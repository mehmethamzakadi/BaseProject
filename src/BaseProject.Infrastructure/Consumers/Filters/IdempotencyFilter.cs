using BaseProject.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BaseProject.Infrastructure.Consumers.Filters;

/// <summary>
/// MassTransit consumer filter - idempotency kontrolü için
/// Consumer'lara mesaj göndermeden önce idempotency kontrolü yapar
/// </summary>
/// <typeparam name="TMessage">Consumer'ın işlediği mesaj tipi</typeparam>
public class IdempotencyFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdempotencyFilter<TMessage>> _logger;
    private readonly string _keyPrefix;
    private readonly Func<TMessage, Guid>? _fallbackIdGenerator;
    private readonly Func<Guid, CancellationToken, Task<bool>>? _existsCheck;

    public IdempotencyFilter(
        IServiceProvider serviceProvider,
        ILogger<IdempotencyFilter<TMessage>> logger,
        string keyPrefix,
        Func<TMessage, Guid>? fallbackIdGenerator = null,
        Func<Guid, CancellationToken, Task<bool>>? existsCheck = null)
    {
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _logger = logger;
        _keyPrefix = keyPrefix;
        _fallbackIdGenerator = fallbackIdGenerator;
        _existsCheck = existsCheck;
    }

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        // Scoped servisi scope içinde çöz
        using var scope = _scopeFactory.CreateScope();
        var idempotencyService = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();

        var messageId = context.MessageId;
        var fallbackId = _fallbackIdGenerator?.Invoke(context.Message);

        // Idempotency kontrolü ve lock alma
        var shouldProcess = await idempotencyService.CheckAndAcquireLockAsync(
            messageId,
            fallbackId,
            _keyPrefix,
            _existsCheck,
            context.CancellationToken);

        if (!shouldProcess)
        {
            _logger.LogInformation(
                "Message already processed (idempotent). MessageId: {MessageId}, MessageType: {MessageType}",
                messageId, typeof(TMessage).Name);
            return; // Skip - already processed (idempotent)
        }

        try
        {
            // Consumer'a devam et
            await next.Send(context);

            // Başarılı işlem sonrası işaretle
            var idempotencyId = messageId ?? fallbackId ?? Guid.Empty;
            if (idempotencyId != Guid.Empty)
            {
                await idempotencyService.MarkAsProcessedAsync(
                    idempotencyId,
                    _keyPrefix,
                    context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing message. MessageId: {MessageId}, MessageType: {MessageType}",
                messageId, typeof(TMessage).Name);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("idempotency");
    }
}
