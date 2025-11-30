namespace BaseProject.Application.Features.Dashboards.Queries.GetAiInsights;

/// <summary>
/// Dashboard için AI destekli içgörüler ve öneriler.
/// </summary>
public sealed record GetAiInsightsResponse
{
    public List<InsightTrend> Trends { get; set; } = new();
    public List<InsightAlert> Alerts { get; set; } = new();
    public List<InsightRecommendation> Recommendations { get; set; } = new();
}

/// <summary>
/// Trend bilgisi - sistem aktivitesindeki değişiklikler.
/// </summary>
public sealed record InsightTrend
{
    public string Type { get; set; } = string.Empty; // user_growth, category_distribution, activity_spike, etc.
    public string Description { get; set; } = string.Empty;
    public string? Metric { get; set; } // Örnek: "+15%", "2x"
    public bool IsPositive { get; set; } // Trend pozitif mi?
}

/// <summary>
/// Uyarı - dikkat edilmesi gereken durumlar.
/// </summary>
public sealed record InsightAlert
{
    public string Severity { get; set; } = string.Empty; // low, medium, high, critical
    public string Message { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
    public DateTime? DetectedAt { get; set; }
}

/// <summary>
/// Öneri - aksiyon alınabilir tavsiyeler.
/// </summary>
public sealed record InsightRecommendation
{
    public string Category { get; set; } = string.Empty; // performance, security, content, user_experience, etc.
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ActionUrl { get; set; } // Önerilen aksiyon için URL
    public int Priority { get; set; } // 1-5 arası, 5 en yüksek öncelik
}
