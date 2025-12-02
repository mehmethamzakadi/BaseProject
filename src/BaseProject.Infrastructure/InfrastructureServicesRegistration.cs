using BaseProject.Application.Abstractions;
using BaseProject.Application.Abstractions.Identity;
using BaseProject.Application.Abstractions.Images;
using BaseProject.Domain.Common.Utilities;
using BaseProject.Domain.Constants;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Events.IntegrationEvents;
using BaseProject.Domain.Repositories;
using BaseProject.Domain.Services;
using BaseProject.Infrastructure.Authorization;
using BaseProject.Infrastructure.Consumers;
using BaseProject.Infrastructure.Consumers.Filters;
using BaseProject.Infrastructure.Options;
using BaseProject.Infrastructure.Services;
using BaseProject.Infrastructure.Services.BackgroundServices.Outbox.Converters;
using BaseProject.Infrastructure.Services.Images;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;
using System.Net;
using System.Text;
using TokenOptions = BaseProject.Application.Options.TokenOptions;

namespace BaseProject.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenOptions>(configuration.GetSection(TokenOptions.SectionName));
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.Configure<PasswordResetOptions>(configuration.GetSection(PasswordResetOptions.SectionName));
            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
            services.Configure<Options.OllamaOptions>(configuration.GetSection(Options.OllamaOptions.SectionName));

            // Custom Password Hasher for User entity
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<AspNetCorePasswordHasher>();
            services.AddScoped<Domain.Services.IPasswordHasher>(sp => sp.GetRequiredService<AspNetCorePasswordHasher>());
            services.AddScoped<Application.Abstractions.Identity.IPasswordHasher>(sp => sp.GetRequiredService<AspNetCorePasswordHasher>());

            TokenOptions tokenOptions = configuration.GetSection(TokenOptions.SectionName).Get<TokenOptions>()
                ?? throw new InvalidOperationException("Token ayarları yapılandırılmalıdır.");

            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
            bool requireHttpsMetadata = !string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

            var redisConnectionString = configuration.GetConnectionString("RedisCache");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                // Redis cache için IDistributedCache register et
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "BaseProject_";
                });

                // IConnectionMultiplexer'ı da register et (SETNX işlemleri için)
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(redisConnectionString));
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddMassTransit(x =>
            {
                x.AddConsumer<ActivityLogConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                    cfg.Host(rabbitOptions.HostName, "/", hostConfigurator =>
                    {
                        hostConfigurator.Username(rabbitOptions.UserName);
                        hostConfigurator.Password(rabbitOptions.Password);
                    });

                    // ✅ OpenTelemetry tracing desteği - Trace ID'yi mesajlara ekle
                    cfg.ConfigureEndpoints(context);

                    // Activity Log queue with retry and error handling
                    cfg.ReceiveEndpoint(EventConstants.ActivityLogQueue, endpointConfigurator =>
                    {
                        endpointConfigurator.ConfigureConsumer<ActivityLogConsumer>(context);

                        // ✅ Idempotency filter ekle - mesaj tekrar işlemeyi önler
                        // Not: IServiceProvider root provider'dan alınır, filter içinde scope oluşturulur
                        var serviceProvider = context; // IBusRegistrationContext is IServiceProvider
                        var scopeFactory = context.GetRequiredService<IServiceScopeFactory>();
                        endpointConfigurator.UseFilter(new IdempotencyFilter<ActivityLogCreatedIntegrationEvent>(
                            serviceProvider,
                            context.GetRequiredService<ILogger<IdempotencyFilter<ActivityLogCreatedIntegrationEvent>>>(),
                            keyPrefix: "idempotency:activitylog:",
                            fallbackIdGenerator: msg => GuidHelper.GenerateDeterministicGuid($"{msg.EntityId}_{msg.Timestamp:O}_{msg.ActivityType}"),
                            existsCheck: async (id, ct) =>
                            {
                                // existsCheck içinde de scope oluşturulmalı
                                using var scope = scopeFactory.CreateScope();
                                var repo = scope.ServiceProvider.GetRequiredService<IActivityLogRepository>();
                                return await repo.ExistsByIdAsync(id, ct);
                            }
                        ));

                        // Retry configuration
                        endpointConfigurator.UseMessageRetry(retryConfigurator =>
                            retryConfigurator.Exponential(5,
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromMinutes(5),
                                TimeSpan.FromSeconds(2)));

                        // Concurrency settings
                        endpointConfigurator.PrefetchCount = 16;
                        endpointConfigurator.ConcurrentMessageLimit = 8;
                    });

                    if (rabbitOptions.RetryLimit > 0)
                    {
                        cfg.UseMessageRetry(retryConfigurator => retryConfigurator.Immediate(rabbitOptions.RetryLimit));
                    }
                });
            });

            services.AddScoped<ActivityLogConsumer>();

            // Background Services
            services.AddHostedService<Services.BackgroundServices.OutboxProcessorService>();
            services.AddHostedService<Services.BackgroundServices.SessionCleanupService>();

            // Register all IIntegrationEventConverterStrategy implementations automatically
            var converterInterface = typeof(IIntegrationEventConverterStrategy);
            var converterTypes = typeof(InfrastructureServicesRegistration).Assembly.GetTypes()
                .Where(t => converterInterface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var impl in converterTypes)
            {
                services.AddScoped(converterInterface, impl);
            }

            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();
            services.AddTransient<ITokenService, JwtTokenService>();
            services.AddTransient<IMailService, MailService>();
            services.AddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IImageStorageService, ImageStorageService>();
            services.AddScoped<IUserDomainService, Domain.Services.UserDomainService>();

            // Ollama AI Service - Best practices: IHttpClientFactory + Polly retry policy
            var ollamaOptions = configuration.GetSection(Options.OllamaOptions.SectionName).Get<Options.OllamaOptions>()
                ?? throw new InvalidOperationException("Ollama ayarları yapılandırılmalıdır.");

            services.AddHttpClient("OllamaClient", client =>
            {
                client.BaseAddress = new Uri(ollamaOptions.Endpoint);
                client.Timeout = TimeSpan.FromMinutes(ollamaOptions.TimeoutMinutes);
            })
            .AddPolicyHandler(GetRetryPolicy(ollamaOptions));

            services.AddScoped<IAiService, AiService>();

            // Authorization
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddAuthorizationCore(options =>
            {
                // Permission'lar için policy'ler oluştur
                foreach (var permission in Permissions.GetAllPermissions())
                {
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            // Register log cleanup background service
            services.AddHostedService<LogCleanupService>();

            return services;
        }

        /// <summary>
        /// Ollama API için retry policy oluşturur.
        /// Best practice: Transient hatalar için exponential backoff retry.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(Options.OllamaOptions options)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // 5xx ve 408 (Request Timeout) hatalarını yakalar
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 429 Rate Limit
                .WaitAndRetryAsync(
                    retryCount: options.RetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(
                        Math.Pow(2, retryAttempt) * options.RetryDelaySeconds), // Exponential backoff
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Logging için (opsiyonel - ILogger inject edilebilir)
                        // Burada sadece policy tanımlanıyor, logging servis içinde yapılıyor
                    });
        }
    }
}