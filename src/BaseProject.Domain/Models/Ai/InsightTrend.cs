namespace BaseProject.Domain.Models.Ai;

/// <summary>
/// Trend bilgisi - sistem aktivitesindeki değişiklikler.
/// </summary>
public sealed record InsightTrend
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Metric { get; init; }
    public bool IsPositive { get; init; }
}
