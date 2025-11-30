namespace BaseProject.Domain.Models.Ai;

/// <summary>
/// Öneri - aksiyon alınabilir tavsiyeler.
/// </summary>
public sealed record InsightRecommendation
{
    public string Category { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public int Priority { get; init; }
}
