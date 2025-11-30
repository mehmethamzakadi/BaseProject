using BaseProject.Domain.Entities;
using BaseProject.Domain.Models.Ai;

namespace BaseProject.Domain.Services;

/// <summary>
/// Yapay zeka destekli içerik üretme servisi için interface.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Verilen kategori adı için SEO uyumlu, kısa bir açıklama üretir.
    /// </summary>
    /// <param name="categoryName">Kategori adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Üretilen kategori açıklaması</returns>
    Task<string> GenerateCategoryDescriptionAsync(string categoryName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dashboard için AI destekli içgörüler, trendler ve öneriler üretir.
    /// </summary>
    /// <param name="statistics">Dashboard istatistikleri (kullanıcı sayısı, kategori sayısı, vb.)</param>
    /// <param name="recentActivities">Son aktivite logları</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>AI tarafından üretilmiş içgörüler, trendler ve öneriler</returns>
    Task<DashboardInsights> GenerateDashboardInsightsAsync(
        DashboardStatistics statistics,
        List<ActivityLog> recentActivities,
        CancellationToken cancellationToken = default);
}
