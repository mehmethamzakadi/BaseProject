namespace BaseProject.Domain.Models.Ai;

/// <summary>
/// Uyarı - dikkat edilmesi gereken durumlar.
/// </summary>
public sealed record InsightAlert
{
    public string Severity { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Suggestion { get; init; }
}
