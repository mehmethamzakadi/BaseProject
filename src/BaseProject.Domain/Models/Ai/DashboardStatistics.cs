namespace BaseProject.Domain.Models.Ai;

/// <summary>
/// Dashboard istatistikleri için veri modeli.
/// </summary>
public sealed record DashboardStatistics
{
    public int TotalCategories { get; init; }
    public int TotalUsers { get; init; }
    public int TotalRoles { get; init; }
}
