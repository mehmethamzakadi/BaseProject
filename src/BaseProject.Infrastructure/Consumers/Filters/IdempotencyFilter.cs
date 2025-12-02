using BaseProject.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BaseProject.Infrastructure.Consumers.Filters;

/// <summary>
/// MassTransit consumer filter - idempotency kontrolü için
/// Consumer'lara mesaj göndermeden önce idempotency kontrolü yapar
/// 
/// Strategy Pattern kullanarak IIdempotencyChecker<TMessage> ile idempotency kontrolü yapar
/// Eğer checker bulunamazsa, idempotency kontrolü atlanır (opsiyonel)
/// </summary>
/// <typeparam name="TMessage">Consumer'ın işlediği mesaj tipi</typeparam>
public class IdempotencyFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdempotencyFilter<TMessage>> _logger;

    public IdempotencyFilter(
        IServiceProvider serviceProvider,
        ILogger<IdempotencyFilter<TMessage>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        // Scoped servisi scope içinde çöz
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // IIdempotencyChecker<TMessage> resolve et (opsiyonel - yoksa idempotency kontrolü atlanır)
        var checker = serviceProvider.GetService<IIdempotencyChecker<TMessage>>();

        if (checker == null)
        {
            // Checker bulunamadı - idempotency kontrolü yapılmadan devam et
            _logger.LogDebug(
                "No IIdempotencyChecker<{MessageType}> found. Skipping idempotency check. MessageId: {MessageId}",
                typeof(TMessage).Name, context.MessageId);
            await next.Send(context);
            return;
        }

        var idempotencyService = serviceProvider.GetRequiredService<IIdempotencyService>();

        var messageId = context.MessageId;
        var idempotencyId = checker.GetIdempotencyId(context.Message, messageId);
        
        // ✅ Versioned ve namespaced key prefix - eski "dirty" key'lerle çakışmayı önler
        // Format: idempotency:v2:{MessageType}:{IdempotencyId}
        // v2: Version number (eski key'lerden ayrım için)
        // MessageType: typeof(TMessage).Name (farklı mesaj tipleri için namespace)
        var messageTypeName = typeof(TMessage).Name;
        var keyPrefix = $"idempotency:v2:{messageTypeName}:";

        // Exists check fonksiyonu - checker'ı kullan
        Func<Guid, CancellationToken, Task<bool>> existsCheck = async (id, ct) =>
            await checker.ExistsAsync(context.Message, id, ct);

        // Idempotency kontrolü ve lock alma
        var shouldProcess = await idempotencyService.CheckAndAcquireLockAsync(
            messageId,
            idempotencyId,
            keyPrefix,
            existsCheck,
            context.CancellationToken);

        if (!shouldProcess)
        {
            _logger.LogInformation(
                "Message already processed (idempotent). MessageId: {MessageId}, IdempotencyId: {IdempotencyId}, MessageType: {MessageType}",
                messageId, idempotencyId, typeof(TMessage).Name);
            return; // Skip - already processed (idempotent)
        }

        try
        {
            // Consumer'a devam et
            await next.Send(context);

            // Başarılı işlem sonrası işaretle
            await idempotencyService.MarkAsProcessedAsync(
                idempotencyId,
                keyPrefix,
                context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing message. MessageId: {MessageId}, IdempotencyId: {IdempotencyId}, MessageType: {MessageType}",
                messageId, idempotencyId, typeof(TMessage).Name);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("idempotency");
    }
}
