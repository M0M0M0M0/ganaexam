using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Models;

namespace SmartShop.Pages;

public class IndexModel(SmartShopDbContext db) : PageModel
{
    public IReadOnlyList<Product> Products { get; private set; } = [];

    public IReadOnlyList<string> Categories { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Products = await db.Products.AsNoTracking().OrderBy(p => p.ProductId).ToListAsync(ct);
        Categories = Products.Select(p => p.Category).Distinct().Order().ToList();
    }
}
