using SmartShop.Data;
using SmartShop.Dtos;
using SmartShop.Models;
using SmartShop.Services.Gemini;

namespace SmartShop.Services;

public class ReviewService(SmartShopDbContext db, IGeminiClient gemini, ILogger<ReviewService> logger)
{
    private static readonly string[] AllowedSentiments = ["Positive", "Negative", "Neutral"];

    public async Task<ReviewResponse?> AnalyzeAndSaveAsync(AnalyzeReviewRequest request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct);
        if (product is null) return null;

        var customerName = request.CustomerName.Trim();
        var content = request.Content.Trim();

        var analysis = await gemini.GenerateJsonAsync<ReviewAnalysis>(
            Prompts.ReviewAnalystSystemInstruction,
            Prompts.ReviewUserPrompt(product, customerName, content, request.Rating),
            Prompts.ReviewAnalysisSchema,
            ct: ct);

        Validate(analysis);

        var review = new Review
        {
            ProductId = product.ProductId,
            CustomerName = customerName,
            Content = content,
            Rating = request.Rating,
            Sentiment = AllowedSentiments.First(s => s.Equals(analysis.Sentiment.Trim(), StringComparison.OrdinalIgnoreCase)),
            Score = Math.Round(Math.Clamp(analysis.Score, 0.0, 1.0), 2),
            Summary = Truncate(analysis.Summary.Trim(), 256),
            CreatedAt = DateTime.Now,
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Saved review {ReviewId} ({Sentiment}) for product {ProductId}",
            review.ReviewId, review.Sentiment, review.ProductId);

        return new ReviewResponse(review.ReviewId, product.ProductId, product.ProductName, review.CustomerName,
            review.Content, review.Rating, review.Sentiment, review.Score, review.Summary, review.CreatedAt);
    }

    private static void Validate(ReviewAnalysis analysis)
    {
        if (!AllowedSentiments.Contains(analysis.Sentiment?.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new GeminiInvalidResponseException($"Gemini returned an invalid sentiment '{analysis.Sentiment}'.");
        if (double.IsNaN(analysis.Score) || analysis.Score < 0 || analysis.Score > 1)
            throw new GeminiInvalidResponseException($"Gemini returned an invalid score '{analysis.Score}'.");
        if (string.IsNullOrWhiteSpace(analysis.Summary))
            throw new GeminiInvalidResponseException("Gemini returned an empty summary.");
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..(max - 3)] + "...";
}
