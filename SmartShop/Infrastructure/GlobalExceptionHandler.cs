using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.Services.Gemini;

namespace SmartShop.Infrastructure;

public class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            GeminiQuotaExceededException => (StatusCodes.Status429TooManyRequests, "Gemini quota exceeded"),
            GeminiInvalidResponseException => (StatusCodes.Status502BadGateway, "Invalid response from Gemini"),
            GeminiNotConfiguredException => (StatusCodes.Status503ServiceUnavailable, "Gemini is not configured"),
            GeminiException => (StatusCodes.Status502BadGateway, "Gemini API error"),
            DbUpdateException => (StatusCodes.Status500InternalServerError, "Database error"),
            OperationCanceledException => (499, "Request cancelled"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        if (status >= 500) logger.LogError(exception, "{Title}", title);
        else logger.LogWarning("{Title}: {Message}", title, exception.Message);

        if (exception is GeminiQuotaExceededException { RetryAfter: { } retryAfter })
            context.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception is GeminiException ? exception.Message : "An unexpected error occurred.",
            },
        });
    }
}
