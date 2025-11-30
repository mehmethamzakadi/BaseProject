using BaseProject.Application.Features.Dashboards.Queries.GetRecentActivities;
using BaseProject.Application.Features.Dashboards.Queries.GetStatistics;
using BaseProject.Domain.Models.Ai;
using BaseProject.Domain.Repositories;
using BaseProject.Domain.Services;
using MediatR;

namespace BaseProject.Application.Features.Dashboards.Queries.GetAiInsights;

/// <summary>
/// Dashboard için AI destekli içgörüler üretir.
/// </summary>
public sealed class GetAiInsightsQueryHandler : IRequestHandler<GetAiInsightsQuery, GetAiInsightsResponse>
{
    private readonly IMediator mediator;
    private readonly IAiService aiService;
    private readonly IActivityLogRepository activityLogRepository;

    public GetAiInsightsQueryHandler(
        IMediator mediator,
        IAiService aiService,
        IActivityLogRepository activityLogRepository)
    {
        this.mediator = mediator;
        this.aiService = aiService;
        this.activityLogRepository = activityLogRepository;
    }

    public async Task<GetAiInsightsResponse> Handle(
        GetAiInsightsQuery request,
        CancellationToken cancellationToken)
    {
        // İstatistikleri ve son aktiviteleri al
        var statisticsResponse = await mediator.Send(new GetStatisticsQuery(), cancellationToken);
        var activitiesResponse = await mediator.Send(new GetRecentActivitiesQuery(50), cancellationToken); // Son 50 aktivite

        // Domain modellerine dönüştür
        var statistics = new DashboardStatistics
        {
            TotalCategories = statisticsResponse.TotalCategories,
            TotalUsers = statisticsResponse.TotalUsers,
            TotalRoles = statisticsResponse.TotalRoles
        };

        // ActivityLog entity'lerini al (AI servisi için)
        var activityLogs = await activityLogRepository.GetRecentActivitiesAsync(50, cancellationToken);

        // AI servisi ile içgörüleri üret
        var insights = await aiService.GenerateDashboardInsightsAsync(
            statistics,
            activityLogs,
            cancellationToken);

        // Application response'a dönüştür
        return new GetAiInsightsResponse
        {
            Trends = insights.Trends.Select(t => new InsightTrend
            {
                Type = t.Type,
                Description = t.Description,
                Metric = t.Metric,
                IsPositive = t.IsPositive
            }).ToList(),
            Alerts = insights.Alerts.Select(a => new InsightAlert
            {
                Severity = a.Severity,
                Message = a.Message,
                Suggestion = a.Suggestion
            }).ToList(),
            Recommendations = insights.Recommendations.Select(r => new InsightRecommendation
            {
                Category = r.Category,
                Title = r.Title,
                Description = r.Description,
                ActionUrl = r.ActionUrl,
                Priority = r.Priority
            }).ToList()
        };
    }
}
