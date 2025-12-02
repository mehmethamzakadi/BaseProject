namespace BaseProject.Application.Abstractions;

/// <summary>
/// Idempotency kontrolü için merkezi servis
/// Consumer'larda mesaj tekrar işlemeyi önlemek için kullanılır
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Mesajın daha önce işlenip işlenmediğini kontrol eder ve lock alır
    /// </summary>
    /// <param name="messageId">Mesaj ID (MassTransit MessageId)</param>
    /// <param name="fallbackId">MessageId yoksa kullanılacak fallback ID</param>
    /// <param name="keyPrefix">Cache key prefix (örn: "idempotency:activitylog:")</param>
    /// <param name="existsCheck">DB'de varlık kontrolü için fonksiyon (opsiyonel)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if message should be processed, False if already processed (idempotent)</returns>
    Task<bool> CheckAndAcquireLockAsync(
        Guid? messageId,
        Guid? fallbackId,
        string keyPrefix,
        Func<Guid, CancellationToken, Task<bool>>? existsCheck = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mesajın başarıyla işlendiğini işaretler
    /// </summary>
    /// <param name="idempotencyId">Idempotency ID (messageId veya fallbackId)</param>
    /// <param name="keyPrefix">Cache key prefix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkAsProcessedAsync(
        Guid idempotencyId,
        string keyPrefix,
        CancellationToken cancellationToken = default);
}
