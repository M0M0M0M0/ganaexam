using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Dtos;

namespace SmartShop.Services;

public class ReportService(SmartShopDbContext db)
{
    public async Task<IReadOnlyList<ReviewReportRow>> GetReviewReportAsync(CancellationToken ct = default)
    {
        var stats = await db.Products
            .AsNoTracking()
            .Where(p => p.Reviews.Any())
            .OrderBy(p => p.ProductId)
            .Select(p => new
            {
                p.ProductName,
                Total = p.Reviews.Count,
                Positive = p.Reviews.Count(r => r.Sentiment == "Positive"),
                Negative = p.Reviews.Count(r => r.Sentiment == "Negative"),
                Neutral = p.Reviews.Count(r => r.Sentiment == "Neutral"),
                Average = p.Reviews.Average(r => (double)r.Rating),
            })
            .ToListAsync(ct);

        return stats
            .Select((s, i) => new ReviewReportRow(
                i + 1, s.ProductName, s.Total, s.Positive, s.Negative, s.Neutral,
                Math.Round(s.Average, 1, MidpointRounding.AwayFromZero)))
            .ToList();
    }
}
