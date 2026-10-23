using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace SmartShop.Services.Gemini;

public interface IGeminiClient
{
    Task<string> GenerateTextAsync(string systemInstruction, IReadOnlyList<ChatMessage> conversation,
        double temperature = 0.7, CancellationToken ct = default);

    Task<T> GenerateJsonAsync<T>(string systemInstruction, string userPrompt, object responseSchema,
        double temperature = 0.2, CancellationToken ct = default);
}

public class GeminiClient(HttpClient http, IOptions<GeminiOptions> options, ILogger<GeminiClient> logger) : IGeminiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly GeminiOptions _options = options.Value;

    public async Task<string> GenerateTextAsync(string systemInstruction, IReadOnlyList<ChatMessage> conversation,
        double temperature = 0.7, CancellationToken ct = default)
    {
        var request = new GenerateContentRequest
        {
            SystemInstruction = Content.FromText(null, systemInstruction),
            Contents = conversation.Select(m => Content.FromText(m.Role, m.Text)).ToList(),
            GenerationConfig = new GenerationConfig { Temperature = temperature },
        };
        return await SendAsync(request, ct);
    }

    public async Task<T> GenerateJsonAsync<T>(string systemInstruction, string userPrompt, object responseSchema,
        double temperature = 0.2, CancellationToken ct = default)
    {
        var request = new GenerateContentRequest
        {
            SystemInstruction = Content.FromText(null, systemInstruction),
            Contents = [Content.FromText(ChatMessage.UserRole, userPrompt)],
            GenerationConfig = new GenerationConfig
            {
                Temperature = temperature,
                ResponseMimeType = "application/json",
                ResponseSchema = responseSchema,
            },
        };

        var text = await SendAsync(request, ct);
        try
        {
            return JsonSerializer.Deserialize<T>(StripCodeFence(text), JsonOptions)
                   ?? throw new GeminiInvalidResponseException("Gemini returned an empty JSON document.");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Gemini returned invalid JSON: {Text}", text);
            throw new GeminiInvalidResponseException("Gemini returned invalid JSON.", ex);
        }
    }

    private async Task<string> SendAsync(GenerateContentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new GeminiNotConfiguredException();

        for (var attempt = 0; ; attempt++)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, $"models/{_options.Model}:generateContent")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json"),
            };
            message.Headers.Add("x-goog-api-key", _options.ApiKey);

            HttpResponseMessage response;
            try
            {
                response = await http.SendAsync(message, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new GeminiException("Could not reach the Gemini API.", ex);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                throw new GeminiException("The Gemini API request timed out.", ex);
            }

            using (response)
            {
                if (IsTransient(response.StatusCode) && attempt < _options.MaxRetries)
                {
                    var delay = GetRetryAfter(response) ?? TimeSpan.FromSeconds(Math.Pow(2, attempt + 1));
                    logger.LogWarning("Gemini returned {Status}; retry {Attempt} in {Delay}s",
                        (int)response.StatusCode, attempt + 1, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new GeminiQuotaExceededException(GetRetryAfter(response));

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    throw new GeminiException("Gemini is busy right now. Please try again in a moment.");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    logger.LogError("Gemini error {Status}: {Body}", (int)response.StatusCode, error);
                    throw new GeminiException($"Gemini API returned HTTP {(int)response.StatusCode}.");
                }

                GenerateContentResponse? body;
                try
                {
                    body = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(JsonOptions, ct);
                }
                catch (JsonException ex)
                {
                    throw new GeminiInvalidResponseException("Could not parse the Gemini API response.", ex);
                }

                return ExtractText(body);
            }
        }
    }

    private static string ExtractText(GenerateContentResponse? body)
    {
        if (body?.PromptFeedback?.BlockReason is { } blockReason)
            throw new GeminiInvalidResponseException($"The prompt was blocked by Gemini ({blockReason}).");

        var candidate = body?.Candidates?.FirstOrDefault();
        var text = string.Concat(candidate?.Content?.Parts
            .Where(p => p.Thought != true)
            .Select(p => p.Text) ?? []);

        if (string.IsNullOrWhiteSpace(text))
            throw new GeminiInvalidResponseException(
                $"Gemini returned no text (finishReason: {candidate?.FinishReason ?? "unknown"}).");

        return text.Trim();
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable;

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter?.Delta;
        return retryAfter is { } d && d > TimeSpan.FromSeconds(10) ? TimeSpan.FromSeconds(10) : retryAfter;
    }

    private static string StripCodeFence(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```")) return trimmed;
        var firstNewLine = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        return firstNewLine > 0 && lastFence > firstNewLine
            ? trimmed[(firstNewLine + 1)..lastFence].Trim()
            : trimmed;
    }
}
