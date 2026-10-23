using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Dtos;
using SmartShop.Services;

namespace SmartShop.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService productService, SmartShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await db.Products.AsNoTracking().OrderBy(p => p.ProductId)
            .Select(p => new { p.ProductId, p.ProductName, p.Category, p.Price, p.Features, p.Description })
            .ToListAsync(ct));

    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        try
        {
            var product = await productService.CreateAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, product);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Product already exists", detail: ex.Message);
        }
    }

    [HttpPost("{id:int}/generatedescription", Name = "generatedescription")]
    [ProducesResponseType<GenerateDescriptionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateDescription(int id, CancellationToken ct)
    {
        var result = await productService.GenerateDescriptionAsync(id, ct);
        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found",
                detail: $"Product with id {id} does not exist.")
            : Ok(result);
    }
}
