using System.Text.Json.Serialization;

namespace BaseProject.Infrastructure.Models.Ollama;

/// <summary>
/// Ollama API chat completion yanıt modeli.
/// </summary>
public sealed class OllamaChatResponse
{
    [JsonPropertyName("message")]
    public OllamaMessage? Message { get; set; }
}
