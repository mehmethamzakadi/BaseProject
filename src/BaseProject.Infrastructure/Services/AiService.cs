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
}
