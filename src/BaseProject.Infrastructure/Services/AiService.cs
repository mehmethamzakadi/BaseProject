using BaseProject.Domain.Entities;
using BaseProject.Domain.Models.Ai;
using BaseProject.Domain.Services;
using BaseProject.Infrastructure.Models.Ollama;
using BaseProject.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace BaseProject.Infrastructure.Services;

/// <summary>
/// Ollama (Qwen 2.5:7b) kullanarak yapay zeka destekli içerik üretme servisi.
/// Best practices: IHttpClientFactory, retry policy, logging, proper error handling.
/// </summary>
public sealed class AiService : IAiService
{
    private const string HttpClientName = "OllamaClient";
    
    private readonly IHttpClientFactory httpClientFactory;
    private readonly OllamaOptions options;
    private readonly ILogger<AiService> logger;

    public AiService(
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> options,
        ILogger<AiService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<string> GenerateCategoryDescriptionAsync(string categoryName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException("Kategori adı boş olamaz.", nameof(categoryName));
        }

        logger.LogInformation("Kategori açıklaması üretiliyor: {CategoryName}", categoryName);

        var systemPrompt = "Sen bir SEO ve içerik uzmanısın. Verilen kategori ismi için Türkçe, ilgi çekici, profesyonel ve maksimum 2 cümlelik kısa bir kategori açıklaması yaz. Sadece açıklamayı döndür, tırnak işareti veya ek metin kullanma.";

        var requestBody = new OllamaChatRequest
        {
            Model = options.ModelId,
            Messages = new[]
            {
                new OllamaMessage { Role = "system", Content = systemPrompt },
                new OllamaMessage { Role = "user", Content = $"Kategori adı: {categoryName}" }
            },
            Stream = false
        };

        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientName);
            
            var response = await httpClient.PostAsJsonAsync("/api/chat", requestBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Ollama API hatası: StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode,
                    errorContent);
                
                throw new HttpRequestException(
                    $"Ollama API hatası: {response.StatusCode}. {errorContent}",
                    null,
                    response.StatusCode);
            }

            var responseContent = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: cancellationToken);
            
            if (responseContent?.Message?.Content == null)
            {
                logger.LogWarning("Ollama'dan boş yanıt alındı");
                throw new InvalidOperationException("Ollama'dan geçerli bir yanıt alınamadı.");
            }

            var description = responseContent.Message.Content.Trim();
            
            // Tırnak işaretlerini temizle (eğer varsa)
            description = description.Trim('"', '\'', '`').Trim();

            logger.LogInformation(
                "Kategori açıklaması başarıyla üretildi: {CategoryName}, Uzunluk={Length}",
                categoryName,
                description.Length);

            return description;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Ollama API timeout: {CategoryName}", categoryName);
            throw new TimeoutException("Ollama API'ye istek zaman aşımına uğradı. Lütfen tekrar deneyin.", ex);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Ollama API HTTP hatası: {CategoryName}", categoryName);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kategori açıklaması üretilirken beklenmeyen hata: {CategoryName}", categoryName);
            throw;
        }
    }

    public async Task<DashboardInsights> GenerateDashboardInsightsAsync(
        DashboardStatistics statistics,
        List<ActivityLog> recentActivities,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Dashboard içgörüleri üretiliyor: Categories={Categories}, Users={Users}, Roles={Roles}, Activities={ActivityCount}",
            statistics.TotalCategories,
            statistics.TotalUsers,
            statistics.TotalRoles,
            recentActivities.Count);

        // Aktivite verilerini özetle
        var activitySummary = SummarizeActivities(recentActivities);
        
        var systemPrompt = @"Sen bir iş zekası ve veri analiz uzmanısın. Verilen dashboard istatistiklerini ve aktivite loglarını analiz ederek içgörüler, trendler ve öneriler üret.

ÖNEMLİ: Yanıtını JSON formatında döndür. Şu yapıyı kullan:
{
  ""trends"": [
    {
      ""type"": ""user_growth"",
      ""description"": ""Açıklama"",
      ""metric"": ""+15%"",
      ""isPositive"": true
    }
  ],
  ""alerts"": [
    {
      ""severity"": ""medium"",
      ""message"": ""Uyarı mesajı"",
      ""suggestion"": ""Öneri""
    }
  ],
  ""recommendations"": [
    {
      ""category"": ""performance"",
      ""title"": ""Başlık"",
      ""description"": ""Açıklama"",
      ""actionUrl"": ""/admin/categories"",
      ""priority"": 5
    }
  ]
}

Trend tipleri: user_growth, category_distribution, activity_spike, content_growth
Alert severity: low, medium, high, critical
Öneri kategorileri: performance, security, content, user_experience, maintenance
Priority: 1-5 arası (5 en yüksek öncelik)

Sadece JSON döndür, başka metin ekleme.";

        var userPrompt = $@"İstatistikler:
- Toplam Kategoriler: {statistics.TotalCategories}
- Toplam Kullanıcılar: {statistics.TotalUsers}
- Toplam Roller: {statistics.TotalRoles}

Son Aktivite Özeti:
{activitySummary}

Bu verilere göre:
1. Önemli trendleri tespit et (büyüme, düşüş, değişimler)
2. Dikkat edilmesi gereken durumları belirle (alerts)
3. Aksiyon alınabilir öneriler üret (recommendations)

Türkçe olarak, kısa ve net içgörüler üret. En fazla 3 trend, 2 alert, 3 recommendation döndür.";

        var requestBody = new OllamaChatRequest
        {
            Model = options.ModelId,
            Messages = new[]
            {
                new OllamaMessage { Role = "system", Content = systemPrompt },
                new OllamaMessage { Role = "user", Content = userPrompt }
            },
            Stream = false
        };

        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientName);
            
            var response = await httpClient.PostAsJsonAsync("/api/chat", requestBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Ollama API hatası (Dashboard Insights): StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode,
                    errorContent);
                
                throw new HttpRequestException(
                    $"Ollama API hatası: {response.StatusCode}. {errorContent}",
                    null,
                    response.StatusCode);
            }

            var responseContent = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: cancellationToken);
            
            if (responseContent?.Message?.Content == null)
            {
                logger.LogWarning("Ollama'dan boş yanıt alındı (Dashboard Insights)");
                return new DashboardInsights(); // Boş içgörüler döndür
            }

            var aiResponse = responseContent.Message.Content.Trim();
            
            // JSON'u parse et
            var insights = ParseDashboardInsights(aiResponse);

            logger.LogInformation(
                "Dashboard içgörüleri başarıyla üretildi: Trends={TrendCount}, Alerts={AlertCount}, Recommendations={RecommendationCount}",
                insights.Trends.Count,
                insights.Alerts.Count,
                insights.Recommendations.Count);

            return insights;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Ollama API timeout (Dashboard Insights)");
            return new DashboardInsights(); // Timeout durumunda boş içgörüler döndür
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dashboard içgörüleri üretilirken hata oluştu");
            // Hata durumunda boş içgörüler döndür, uygulama çalışmaya devam etsin
            return new DashboardInsights();
        }
    }

    private static string SummarizeActivities(List<ActivityLog> activities)
    {
        if (activities.Count == 0)
            return "Son dönemde aktivite kaydı bulunmuyor.";

        var activityTypes = activities
            .GroupBy(a => a.ActivityType)
            .Select(g => $"- {g.Key}: {g.Count()} kez")
            .ToList();

        var entityTypes = activities
            .GroupBy(a => a.EntityType)
            .Select(g => $"- {g.Key}: {g.Count()} kez")
            .ToList();

        var summary = $"Son {activities.Count} aktivite:\n";
        summary += $"Aktivite Tipleri:\n{string.Join("\n", activityTypes)}\n";
        summary += $"Entity Tipleri:\n{string.Join("\n", entityTypes)}";

        return summary;
    }

    private DashboardInsights ParseDashboardInsights(string aiResponse)
    {
        try
        {
            // AI yanıtından JSON'u çıkar (eğer markdown code block içindeyse)
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            
            if (jsonStart < 0 || jsonEnd < 0 || jsonEnd <= jsonStart)
            {
                logger.LogWarning("AI yanıtında geçerli JSON bulunamadı: {Response}", aiResponse);
                return new DashboardInsights();
            }

            var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
            
            // JSON'u parse et
            using var document = System.Text.Json.JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            var trends = new List<InsightTrend>();
            if (root.TryGetProperty("trends", out var trendsElement) && trendsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var trendElement in trendsElement.EnumerateArray())
                {
                    trends.Add(new InsightTrend
                    {
                        Type = trendElement.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty,
                        Description = trendElement.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                        Metric = trendElement.TryGetProperty("metric", out var metricProp) ? metricProp.GetString() : null,
                        IsPositive = trendElement.TryGetProperty("isPositive", out var positiveProp) && positiveProp.GetBoolean()
                    });
                }
            }

            var alerts = new List<InsightAlert>();
            if (root.TryGetProperty("alerts", out var alertsElement) && alertsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var alertElement in alertsElement.EnumerateArray())
                {
                    alerts.Add(new InsightAlert
                    {
                        Severity = alertElement.TryGetProperty("severity", out var severityProp) ? severityProp.GetString() ?? "medium" : "medium",
                        Message = alertElement.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? string.Empty : string.Empty,
                        Suggestion = alertElement.TryGetProperty("suggestion", out var suggProp) ? suggProp.GetString() : null
                    });
                }
            }

            var recommendations = new List<InsightRecommendation>();
            if (root.TryGetProperty("recommendations", out var recsElement) && recsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var recElement in recsElement.EnumerateArray())
                {
                    recommendations.Add(new InsightRecommendation
                    {
                        Category = recElement.TryGetProperty("category", out var catProp) ? catProp.GetString() ?? string.Empty : string.Empty,
                        Title = recElement.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty,
                        Description = recElement.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                        ActionUrl = recElement.TryGetProperty("actionUrl", out var urlProp) ? urlProp.GetString() : null,
                        Priority = recElement.TryGetProperty("priority", out var prioProp) && prioProp.ValueKind == System.Text.Json.JsonValueKind.Number 
                            ? prioProp.GetInt32() : 3
                    });
                }
            }

            return new DashboardInsights
            {
                Trends = trends,
                Alerts = alerts,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI yanıtını parse ederken hata oluştu: {Response}", aiResponse);
            return new DashboardInsights();
        }
    }
}
