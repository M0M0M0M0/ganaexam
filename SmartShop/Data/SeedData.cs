using SmartShop.Models;

namespace SmartShop.Data;

public static class SeedData
{
    public static readonly Product[] Products =
    [
        new() { ProductId = 1, ProductName = "Galaxy S25", Category = "Phone", Price = 22_990_000m,
                Features = "6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera" },
        new() { ProductId = 2, ProductName = "MacBook Air M4", Category = "Laptop", Price = 28_490_000m,
                Features = "13.6 inch, 16GB RAM, 18 hours battery" },
        new() { ProductId = 3, ProductName = "AirPods Pro 3", Category = "Accessory", Price = 6_790_000m,
                Features = "Active noise cancellation, USB-C, IP57" },
        new() { ProductId = 4, ProductName = "Xiaomi Redmi Note 15", Category = "Phone", Price = 6_490_000m,
                Features = "120Hz display, 5500mAh battery, 108MP camera" },
    ];

    private static readonly DateTime SeedDate = new(2026, 1, 1, 9, 0, 0);

    public static readonly Review[] Reviews =
    [
        Make(1, 1, "Nguyen An", "Screen is gorgeous and the phone is very fast.", 5, "Positive", 0.95, "Gorgeous screen and very fast performance."),
        Make(2, 1, "Tran Binh", "Camera takes amazing photos even at night.", 5, "Positive", 0.93, "Excellent camera, great night photos."),
        Make(3, 1, "Le Chi", "Great phone overall, battery could be better.", 5, "Positive", 0.81, "Great phone, battery could be better."),
        Make(4, 1, "Pham Dung", "Battery is great but the phone gets hot when gaming.", 2, "Negative", 0.72, "Phone overheats when gaming."),
        Make(5, 1, "Hoang Em", "It is okay, nothing special for the price.", 3, "Neutral", 0.66, "Average phone for its price."),
        Make(6, 2, "Vo Giang", "Battery lasts all day and it is super light.", 5, "Positive", 0.96, "All-day battery and very lightweight."),
        Make(7, 2, "Dang Hai", "Silent, fast and the display is beautiful.", 5, "Positive", 0.94, "Silent, fast laptop with a beautiful display."),
        Make(8, 2, "Bui Khanh", "Very good laptop, only wish it had more ports.", 4, "Positive", 0.84, "Very good laptop but few ports."),
        Make(9, 3, "Do Lan", "Left earbud stopped working after one week.", 1, "Negative", 0.95, "Left earbud failed after a week."),
        Make(10, 3, "Ngo Minh", "Noise cancellation is weak and they fall out of my ears.", 2, "Negative", 0.88, "Weak noise cancellation and poor fit."),
    ];

    private static Review Make(int id, int productId, string customer, string content, int rating,
        string sentiment, double score, string summary) => new()
    {
        ReviewId = id,
        ProductId = productId,
        CustomerName = customer,
        Content = content,
        Rating = rating,
        Sentiment = sentiment,
        Score = score,
        Summary = summary,
        CreatedAt = SeedDate.AddDays(id),
    };
}
