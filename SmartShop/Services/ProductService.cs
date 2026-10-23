using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Dtos;
using SmartShop.Models;
using SmartShop.Services.Gemini;

namespace SmartShop.Services;

public class ProductService(SmartShopDbContext db, IGeminiClient gemini, ILogger<ProductService> logger)
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var name = request.ProductName.Trim();
        if (await db.Products.AnyAsync(p => p.ProductName == name, ct))
            throw new InvalidOperationException($"A product named '{name}' already exists.");

        var nextId = (await db.Products.MaxAsync(p => (int?)p.ProductId, ct) ?? 0) + 1;
        var product = new Product
        {
            ProductId = nextId,
            ProductName = name,
            Category = request.Category.Trim(),
            Price = request.Price,
            Features = string.Join(", ", request.Features
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)),
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created product {ProductId} ({ProductName})", product.ProductId, product.ProductName);

        return new ProductResponse(product.ProductId, product.ProductName, product.Category,
            product.Price, product.Features, product.Description);
    }

    public async Task<GenerateDescriptionResponse?> GenerateDescriptionAsync(int productId, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([productId], ct);
        if (product is null) return null;

        var text = await gemini.GenerateTextAsync(
            Prompts.CopywriterSystemInstruction,
            [new ChatMessage(ChatMessage.UserRole, Prompts.DescriptionUserPrompt(product))],
            temperature: 0.8,
            ct: ct);

        var description = LimitWords(text, Prompts.MaxDescriptionWords);
        product.Description = description;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Generated description for product {ProductId}", productId);
        return new GenerateDescriptionResponse(product.ProductId, product.ProductName, description, CountWords(description));
    }

    private static string[] Words(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static int CountWords(string text) => Words(text).Length;

    private static string LimitWords(string text, int maxWords)
    {
        var cleaned = text.Trim().Trim('"');
        var words = Words(cleaned);
        if (words.Length <= maxWords) return string.Join(' ', words);
        var cut = string.Join(' ', words.Take(maxWords)).TrimEnd(',', ';', ':');
        return cut.EndsWith('.') ? cut : cut + "...";
    }
}
