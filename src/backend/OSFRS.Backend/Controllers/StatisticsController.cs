using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StatisticsController : ControllerBase
{
    private readonly IUsageService _usage;
    private readonly IReportService _report;
    private readonly IAppLogger<StatisticsController> _logger;

    public StatisticsController(IUsageService usage, IReportService report, IAppLogger<StatisticsController> logger)
    {
        _usage = usage;
        _report = report;
        _logger = logger;
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? eventType,
        [FromQuery] int? userId,
        [FromQuery] int? facilityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            var events = await _usage.GetEventsAsync(
                eventType,
                userId,
                facilityId,
                from,
                to
            );
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching usage events.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpGet("aggregate/daily")]
    public async Task<IActionResult> GetDailyAggregate([FromQuery] DateTime? date)
    {
        try
        {
            var targetDate = date ?? DateTime.UtcNow;

            var results = await _usage.GetDailyAggregateAsync(targetDate);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily analytics.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpGet("aggregate/monthly")]
    public async Task<IActionResult> GetMonthlyAggregate([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            int targetYear = year ?? DateTime.UtcNow.Year;
            int targetMonth = month ?? DateTime.UtcNow.Month;

            var results = await _usage.GetMonthlyAggregateAsync(targetYear, targetMonth);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly analytics.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpPost("aggregate/run")]
    public async Task<IActionResult> RunAggregation()
    {
        try
        {
            await _usage.AggregateAsync();
            return Ok(new { message = "Aggregation executed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running aggregation job.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpGet("reports/daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
    {
        try
        {
            var report = await _report.GetDailyReportAsync(date);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily report");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var report = await _report.GetMonthlyReportAsync(year, month);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateTime? date)
    {
        try
        {
            var bytes = await _report.ExportCsvAsync(date);
            return File(bytes, "text/csv", "usage_report.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting CSV report");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] DateTime? date)
    {
        try
        {
            var bytes = await _report.ExportPdfAsync(date);
            return File(bytes, "application/pdf", "usage_report.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF report");
            return StatusCode(500, "Internal server error.");
        }
    }
}