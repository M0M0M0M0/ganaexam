using System.ComponentModel.DataAnnotations;

namespace SmartShop.Dtos;

public class CreateProductRequest
{
    [Required, StringLength(128, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    [Required, StringLength(64, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "9999999999999999", ErrorMessage = "price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required, StringLength(512, MinimumLength = 1)]
    public string Features { get; set; } = string.Empty;
}

public record ProductResponse(
    int ProductId, string ProductName, string Category, decimal Price, string Features, string? Description);

public record GenerateDescriptionResponse(int ProductId, string ProductName, string Description, int WordCount);

public class AnalyzeReviewRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "productId must be a positive number.")]
    public int ProductId { get; set; }

    [Required, StringLength(64, MinimumLength = 1)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "rating must be between 1 and 5.")]
    public int Rating { get; set; }
}

public class ReviewAnalysis
{
    public string Sentiment { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public record ReviewResponse(
    int ReviewId, int ProductId, string ProductName, string CustomerName, string Content, int Rating,
    string Sentiment, double Score, string Summary, DateTime CreatedAt);

public class ChatRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string SessionId { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}

public record ChatHistoryItem(string Role, string Text);

public record ChatResponse(string SessionId, string Reply, IReadOnlyList<ChatHistoryItem> History);

public record ReviewReportRow(
    int No, string ProductName, int TotalReviews, int Positive, int Negative, int Neutral, double AverageRating);
