using System.Text.Json.Serialization;

namespace SmartShop.Services.Gemini;

public record ChatMessage(string Role, string Text)
{
    public const string UserRole = "user";
    public const string ModelRole = "model";
}

internal sealed class GenerateContentRequest
{
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; init; }

    [JsonPropertyName("contents")]
    public required List<Content> Contents { get; init; }

    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; init; }
}

internal sealed class Content
{
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Role { get; init; }

    [JsonPropertyName("parts")]
    public List<Part> Parts { get; init; } = [];

    public static Content FromText(string? role, string text) => new() { Role = role, Parts = [new Part { Text = text }] };
}

internal sealed class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("thought")]
    public bool? Thought { get; init; }
}

internal sealed class GenerationConfig
{
    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("responseMimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResponseMimeType { get; init; }

    [JsonPropertyName("responseSchema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ResponseSchema { get; init; }
}

internal sealed class GenerateContentResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; init; }

    [JsonPropertyName("promptFeedback")]
    public PromptFeedback? PromptFeedback { get; init; }
}

internal sealed class Candidate
{
    [JsonPropertyName("content")]
    public Content? Content { get; init; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; init; }
}

internal sealed class PromptFeedback
{
    [JsonPropertyName("blockReason")]
    public string? BlockReason { get; init; }
}
