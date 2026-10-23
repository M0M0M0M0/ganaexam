using System.Globalization;
using System.Text;
using SmartShop.Models;

namespace SmartShop.Services;

public static class Prompts
{
    public const int MaxDescriptionWords = 80;

    public const string CopywriterSystemInstruction =
        """
        You are a professional e-commerce copywriter for Smart Shop, an online electronics store.
        Write persuasive, accurate product descriptions for customers.
        Rules:
        - Use ONLY the information provided (name, category, features). Never invent specifications, prices or promotions.
        - Write at most 80 words, in one paragraph, in English.
        - Highlight the customer benefit of each feature with a friendly, professional tone.
        - Return only the description text: no title, no markdown, no bullet points, no quotes.
        """;

    public static string DescriptionUserPrompt(Product product) =>
        $"""
        Write a product description (maximum 80 words) for this product:
        Product name: {product.ProductName}
        Category: {product.Category}
        Features: {product.Features}
        """;

    public const string ReviewAnalystSystemInstruction =
        """
        You are a customer review analyst for Smart Shop, an online electronics store.
        Analyze the sentiment of the customer review you receive.
        Rules:
        - sentiment must be exactly one of: "Positive", "Negative", "Neutral".
        - score is your confidence in that sentiment, a number from 0.0 to 1.0.
        - summary is one short English sentence (maximum 20 words) describing the main points of the review.
        - Judge mainly from the review text; use the star rating only as a supporting signal.
        - Respond with JSON only, following the given schema.
        """;

    public static string ReviewUserPrompt(Product product, string customerName, string content, int rating) =>
        $"""
        Analyze this customer review.
        Product: {product.ProductName} ({product.Category})
        Customer: {customerName}
        Rating: {rating}/5 stars
        Review: "{content}"
        """;

    public static readonly object ReviewAnalysisSchema = new
    {
        type = "OBJECT",
        properties = new
        {
            sentiment = new { type = "STRING", @enum = new[] { "Positive", "Negative", "Neutral" } },
            score = new { type = "NUMBER", description = "Confidence score from 0.0 to 1.0" },
            summary = new { type = "STRING", description = "Short summary of the review" },
        },
        required = new[] { "sentiment", "score", "summary" },
        propertyOrdering = new[] { "sentiment", "score", "summary" },
    };

    public static string ShoppingAssistantSystemInstruction(IEnumerable<Product> products)
    {
        var catalog = new StringBuilder();
        foreach (var p in products)
        {
            catalog.AppendLine(
                $"- Id: {p.ProductId} | Name: {p.ProductName} | Category: {p.Category} | " +
                $"Price: {p.Price.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} VND | Features: {p.Features}" +
                (string.IsNullOrWhiteSpace(p.Description) ? "" : $" | Description: {p.Description}"));
        }

        return
            $"""
            You are the AI shopping assistant of Smart Shop, an online electronics store.
            You ONLY answer questions about the products in the PRODUCT CATALOG below
            (features, prices, comparisons, recommendations among these products).

            Rules:
            1. Use only the data in the PRODUCT CATALOG. Never invent products, prices, specifications, stock or promotions.
               If the catalog does not contain the information, say you do not have that information.
            2. If the question is not related to Smart Shop products (e.g. weather, politics, coding, math, other stores),
               politely refuse with: "Sorry, I can only help with questions about Smart Shop products."
            3. Use the previous messages of the conversation to understand follow-up questions
               (e.g. "How much does it cost?" refers to the product discussed before).
            4. Prices are in VND. Answer briefly (at most 3 sentences), in the same language as the customer.

            PRODUCT CATALOG:
            {catalog}
            """;
    }
}
