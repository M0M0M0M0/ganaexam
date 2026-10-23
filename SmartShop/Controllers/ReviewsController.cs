using Microsoft.AspNetCore.Mvc;
using SmartShop.Dtos;
using SmartShop.Services;

namespace SmartShop.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(ReviewService reviewService) : ControllerBase
{
    [HttpPost("analyze", Name = "analyzereview")]
    [ProducesResponseType<ReviewResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeReviewRequest request, CancellationToken ct)
    {
        var result = await reviewService.AnalyzeAndSaveAsync(request, ct);
        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found",
                detail: $"Product with id {request.ProductId} does not exist.")
            : StatusCode(StatusCodes.Status201Created, result);
    }
}
