using BaseProject.Application.Abstractions;
using BaseProject.Domain.Common.Utilities;
using BaseProject.Domain.Events.IntegrationEvents;
using BaseProject.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BaseProject.Infrastructure.Consumers.Checkers;

/// <summary>
/// ActivityLogCreatedIntegrationEvent için idempotency checker implementasyonu
/// </summary>
public class ActivityLogIdempotencyChecker : IIdempotencyChecker<ActivityLogCreatedIntegrationEvent>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<ActivityLogIdempotencyChecker> _logger;
    private const string KeyPrefix = "idempotency:activitylog:";

    public ActivityLogIdempotencyChecker(
        IActivityLogRepository activityLogRepository,
        ILogger<ActivityLogIdempotencyChecker> logger)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
    }

    public async Task<bool> ExistsAsync(
        ActivityLogCreatedIntegrationEvent message,
        Guid idempotencyId,
        CancellationToken cancellationToken)
    {
        // ActivityLog tablosunda bu ID'ye sahip kayıt var mı kontrol et
        var exists = await _activityLogRepository.ExistsByIdAsync(idempotencyId, cancellationToken);

        if (exists)
        {
            _logger.LogDebug(
                "ActivityLog already exists in database. IdempotencyId: {IdempotencyId}, ActivityType: {ActivityType}",
                idempotencyId, message.ActivityType);
        }

        return exists;
    }

    public Guid GetIdempotencyId(ActivityLogCreatedIntegrationEvent message, Guid? messageId)
    {
        // MessageId varsa onu kullan, yoksa deterministic GUID oluştur
        return messageId ?? GuidHelper.GenerateDeterministicGuid(
            $"{message.EntityId}_{message.Timestamp:O}_{message.ActivityType}");
    }

    public string GetKeyPrefix() => KeyPrefix;
}
