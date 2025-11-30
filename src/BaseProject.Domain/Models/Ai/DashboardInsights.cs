namespace BaseProject.Domain.Models.Ai;

/// <summary>
/// Dashboard AI içgörüleri sonuç modeli.
/// </summary>
public sealed record DashboardInsights
{
    public List<InsightTrend> Trends { get; init; } = new();
    public List<InsightAlert> Alerts { get; init; } = new();
    public List<InsightRecommendation> Recommendations { get; init; } = new();
}
