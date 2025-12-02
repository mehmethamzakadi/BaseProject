using BaseProject.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BaseProject.Infrastructure.Services;

/// <summary>
/// Idempotency kontrolü için merkezi servis implementasyonu
/// Redis cache ve database kontrolü ile mesaj tekrar işlemeyi önler
/// </summary>
public class IdempotencyService : IIdempotencyService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<IdempotencyService> _logger;
    private const int LockRetryDelayMs = 100;
    private const int CacheExpirationDays = 7;

    public IdempotencyService(
        ICacheService cacheService,
        ILogger<IdempotencyService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> CheckAndAcquireLockAsync(
        Guid? messageId,
        Guid? fallbackId,
        string keyPrefix,
        Func<Guid, CancellationToken, Task<bool>>? existsCheck = null,
        CancellationToken cancellationToken = default)
    {
        var idempotencyId = messageId ?? fallbackId
            ?? throw new ArgumentException("Either messageId or fallbackId must be provided");

        var idempotencyKey = $"{keyPrefix}{idempotencyId}";

        // 1. Redis'te kontrol et (hızlı kontrol)
        var isProcessedInCache = await _cacheService.AnyAsync(idempotencyKey);
        if (isProcessedInCache)
        {
            _logger.LogInformation(
                "Duplicate message detected in cache. IdempotencyId: {IdempotencyId}, Key: {Key}",
                idempotencyId, idempotencyKey);
            return false; // Already processed - idempotent
        }

        // 2. DB'de kontrol et (fallback - Redis down olabilir)
        if (existsCheck != null)
        {
            var existsInDb = await existsCheck(idempotencyId, cancellationToken);
            if (existsInDb)
            {
                // Cache warming - DB'de var ama Redis'te yok
                await _cacheService.Add(
                    idempotencyKey,
                    true,
                    DateTimeOffset.UtcNow.AddDays(CacheExpirationDays),
                    null);

                _logger.LogInformation(
                    "Duplicate message detected in database. IdempotencyId: {IdempotencyId}",
                    idempotencyId);
                return false; // Already processed - idempotent
            }
        }

        // 3. Redis'e lock al (optimistic lock - atomic işlem)
        var lockAcquired = await _cacheService.AddIfNotExists(
            idempotencyKey,
            true,
            DateTimeOffset.UtcNow.AddDays(CacheExpirationDays),
            null);

        if (!lockAcquired)
        {
            _logger.LogWarning(
                "Could not acquire lock. IdempotencyId: {IdempotencyId}. Another consumer may be processing.",
                idempotencyId);

            // Kısa bekleme ve tekrar kontrol (race condition durumu)
            await Task.Delay(LockRetryDelayMs, cancellationToken);

            if (existsCheck != null)
            {
                var stillExists = await existsCheck(idempotencyId, cancellationToken);
                if (stillExists)
                {
                    _logger.LogInformation(
                        "Message was processed by another consumer. IdempotencyId: {IdempotencyId}",
                        idempotencyId);
                    return false; // Already processed - idempotent
                }
            }
        }

        return true; // Should process - lock acquired
    }

    public async Task MarkAsProcessedAsync(
        Guid idempotencyId,
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = $"{keyPrefix}{idempotencyId}";

        // Redis'i güncelle (zaten eklenmiş olabilir ama emin olmak için)
        await _cacheService.Add(
            idempotencyKey,
            true,
            DateTimeOffset.UtcNow.AddDays(CacheExpirationDays),
            null);
    }
}
