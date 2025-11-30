using BaseProject.Domain.Common;
using BaseProject.Domain.Repositories;
using BaseProject.Persistence.Contexts;
using BaseProject.Persistence.DatabaseInitializer;
using BaseProject.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseProject.Persistence;

public static class PersistenceServicesRegistration
{
    public static IServiceCollection AddConfigurePersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region DbContext Yapılandırması
        var postgreSqlConnectionString = configuration.GetConnectionString("BaseProjectPostgreConnectionString");

        services.AddDbContext<BaseProjectDbContext>((sp, options) =>
        {
            options.UseNpgsql(postgreSqlConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MaxBatchSize(100);
                npgsqlOptions.CommandTimeout(30);
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            options.EnableServiceProviderCaching();
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        #endregion

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshSessionRepository, RefreshSessionRepository>();
        services.AddScoped<IDbInitializer, DbInitializer>();

        // Unit of Work kaydı
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
