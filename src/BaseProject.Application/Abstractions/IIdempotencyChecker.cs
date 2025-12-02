namespace BaseProject.Application.Abstractions;

/// <summary>
/// Idempotency kontrolü için mesaj bazlı checker interface
/// Her mesaj tipi için özel bir checker implementasyonu oluşturulabilir
/// </summary>
/// <typeparam name="TMessage">Kontrol edilecek mesaj tipi</typeparam>
public interface IIdempotencyChecker<in TMessage>
    where TMessage : class
{
    /// <summary>
    /// Mesajın daha önce işlenip işlenmediğini kontrol eder
    /// </summary>
    /// <param name="message">Kontrol edilecek mesaj</param>
    /// <param name="idempotencyId">Idempotency ID (MessageId veya fallback ID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if message was already processed, False otherwise</returns>
    Task<bool> ExistsAsync(TMessage message, Guid idempotencyId, CancellationToken cancellationToken);

    /// <summary>
    /// Mesaj için idempotency ID oluşturur (MessageId yoksa fallback)
    /// </summary>
    /// <param name="message">Mesaj</param>
    /// <param name="messageId">MassTransit MessageId (nullable)</param>
    /// <returns>Idempotency ID</returns>
    Guid GetIdempotencyId(TMessage message, Guid? messageId);

    /// <summary>
    /// Cache key prefix'i döndürür
    /// </summary>
    /// <returns>Key prefix (örn: "idempotency:activitylog:")</returns>
    string GetKeyPrefix();
}
