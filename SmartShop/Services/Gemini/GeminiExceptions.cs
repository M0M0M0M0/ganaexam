namespace SmartShop.Services.Gemini;

public class GeminiException(string message, Exception? inner = null) : Exception(message, inner);

public class GeminiNotConfiguredException()
    : GeminiException("Gemini API key is not configured.");

public class GeminiQuotaExceededException(TimeSpan? retryAfter)
    : GeminiException("Gemini quota exceeded (HTTP 429). Please try again later.")
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

public class GeminiInvalidResponseException(string message, Exception? inner = null) : GeminiException(message, inner);
