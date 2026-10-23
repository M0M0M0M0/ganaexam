using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Dtos;
using SmartShop.Models;
using SmartShop.Services;

namespace SmartShop.Pages;

public class ReportsModel(ReportService reportService, SmartShopDbContext db) : PageModel
{
    public IReadOnlyList<ReviewReportRow> Rows { get; private set; } = [];
    public IReadOnlyList<Product> Products { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Rows = await reportService.GetReviewReportAsync(ct);
        Products = await db.Products.AsNoTracking().OrderBy(p => p.ProductId).ToListAsync(ct);
    }
}
