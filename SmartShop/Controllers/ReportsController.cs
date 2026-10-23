using Microsoft.AspNetCore.Mvc;
using SmartShop.Dtos;
using SmartShop.Services;

namespace SmartShop.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ReportService reportService) : ControllerBase
{
    [HttpGet("reviews", Name = "getreviewreport")]
    [ProducesResponseType<IReadOnlyList<ReviewReportRow>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewReport(CancellationToken ct) =>
        Ok(await reportService.GetReviewReportAsync(ct));
}
