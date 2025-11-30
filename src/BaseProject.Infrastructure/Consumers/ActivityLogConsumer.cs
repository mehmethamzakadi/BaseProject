using BaseProject.Application.Abstractions;
using BaseProject.Domain.Common;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Events.IntegrationEvents;
using BaseProject.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BaseProject.Infrastructure.Consumers;

/// <summary>
/// Consumes ActivityLogCreatedIntegrationEvent from RabbitMQ
/// and persists the activity log to the database
/// </summary>
public class ActivityLogConsumer : IConsumer<ActivityLogCreatedIntegrationEvent>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivityLogConsumer> _logger;
    private readonly ICacheService _cacheService;
    private const string IdempotencyKeyPrefix = "idempotency:activitylog:";

    public ActivityLogConsumer(
        IActivityLogRepository activityLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivityLogConsumer> logger,
        ICacheService cacheService)
    {
        _activityLogRepository = activityLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Consume(ConsumeContext<ActivityLogCreatedIntegrationEvent> context)
    {
        try
        {
            var message = context.Message;

            // ✅ Idempotency kontrolü - MessageId kullan (MassTransit otomatik oluşturur)
            // Eğer MessageId yoksa (çok nadir), EntityId + Timestamp kombinasyonunu hash'le
            Guid activityLogId;
            
            if (context.MessageId.HasValue)
            {
                activityLogId = context.MessageId.Value;
            }
            else
            {
                // Fallback: Deterministic ID oluştur (EntityId + Timestamp + ActivityType)
                var deterministicString = $"{message.EntityId}_{message.Timestamp:O}_{message.ActivityType}";
                activityLogId = GenerateDeterministicGuid(deterministicString);
                
                _logger.LogWarning(
                    "MessageId not available, generated deterministic ID: {ActivityLogId}",
                    activityLogId);
            }

            // ✅ Idempotency kontrolü: Önce Redis'te kontrol et (hızlı), sonra veritabanında kontrol et
            var idempotencyKey = $"{IdempotencyKeyPrefix}{activityLogId}";
            
            // Redis'te kontrol et (hızlı kontrol)
            var isProcessedInCache = await _cacheService.AnyAsync(idempotencyKey);
            if (isProcessedInCache)
            {
                _logger.LogInformation(
                    "Duplicate message detected in cache for ActivityLog {ActivityLogId}. Skipping processing (idempotent).",
                    activityLogId);
                return; // ✅ Idempotent - Redis'te zaten işlenmiş olarak işaretlenmiş
            }

            // Redis'te yoksa veritabanında kontrol et (fallback)
            var existsInDb = await _activityLogRepository.ExistsByIdAsync(activityLogId, context.CancellationToken);
            if (existsInDb)
            {
                // Veritabanında var ama Redis'te yok - Redis'i güncelle (cache warming)
                await _cacheService.Add(
                    idempotencyKey,
                    true,
                    DateTimeOffset.UtcNow.AddDays(7), // 7 gün boyunca sakla
                    null);
                
                _logger.LogInformation(
                    "Duplicate message detected in database for ActivityLog {ActivityLogId}. Skipping processing (idempotent).",
                    activityLogId);
                return; // ✅ Idempotent - veritabanında zaten işlenmiş
            }

            // ✅ OpenTelemetry Trace ID'yi loglara ekle
            var activity = Activity.Current;
            var traceId = activity?.TraceId.ToString() ?? "unknown";
            var spanId = activity?.SpanId.ToString() ?? "unknown";

            _logger.LogInformation(
                "Processing ActivityLog: {ActivityType} for {EntityType} (ID: {EntityId}) [TraceId: {TraceId}, SpanId: {SpanId}]",
                message.ActivityType,
                message.EntityType,
                message.EntityId,
                traceId,
                spanId);

            var activityLog = new ActivityLog
            {
                Id = activityLogId, // ✅ Deterministic ID
                ActivityType = message.ActivityType,
                EntityType = message.EntityType,
                EntityId = message.EntityId,
                Title = message.Title,
                Details = message.Details,
                UserId = message.UserId ?? Guid.Empty,
                Timestamp = message.Timestamp
            };

            await _activityLogRepository.AddAsync(activityLog, context.CancellationToken);
            
            // ✅ UnitOfWork ile transaction yönetimi
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            // ✅ İşlem başarılı olduğunda Redis'e idempotency key'i ekle
            // Bu, aynı mesajın tekrar gelmesi durumunda hızlı kontrol sağlar
            await _cacheService.Add(
                idempotencyKey,
                true,
                DateTimeOffset.UtcNow.AddDays(7), // 7 gün boyunca sakla
                null);

            _logger.LogInformation(
                "Successfully processed ActivityLog: {ActivityType} for {EntityType} (ID: {ActivityLogId})",
                message.ActivityType,
                message.EntityType,
                activityLogId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ActivityLog: {ActivityType}",
                context.Message.ActivityType);
            throw; // Let MassTransit handle retry logic
        }
    }

    /// <summary>
    /// String'den deterministic Guid oluşturur (MD5 hash kullanarak)
    /// Aynı string her zaman aynı Guid'i üretir
    /// </summary>
    private static Guid GenerateDeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
