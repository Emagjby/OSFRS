using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StatisticsController : ControllerBase
{
    private readonly IUsageService _usage;
    private readonly IReportService _report;
    private readonly IAnalyticsService _analytics;
    private readonly IAppLogger<StatisticsController> _logger;

    public StatisticsController(IUsageService usage, IReportService report, IAnalyticsService analytics, IAppLogger<StatisticsController> logger)
    {
        _usage = usage;
        _report = report;
        _analytics = analytics;
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
        var events = await _usage.GetEventsAsync(
            eventType,
            userId,
            facilityId,
            from,
            to
        );
        return Ok(events);
    }

    [HttpGet("aggregate/daily")]
    public async Task<IActionResult> GetDailyAggregate([FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow;

        var results = await _usage.GetDailyAggregateAsync(targetDate);
        return Ok(results);
    }

    [HttpGet("aggregate/monthly")]
    public async Task<IActionResult> GetMonthlyAggregate([FromQuery] int? year, [FromQuery] int? month)
    {
        int targetYear = year ?? DateTime.UtcNow.Year;
        int targetMonth = month ?? DateTime.UtcNow.Month;

        var results = await _usage.GetMonthlyAggregateAsync(targetYear, targetMonth);
        return Ok(results);
    }

    [HttpPost("aggregate/run")]
    public async Task<IActionResult> RunAggregation()
    {
        await _usage.AggregateAsync();
        return Ok(new { message = "Aggregation executed successfully." });
    }

    [HttpGet("reports/daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
    {
        var report = await _report.GetDailyReportAsync(date);
        return Ok(report);
    }

    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int? year, [FromQuery] int? month)
    {
        var report = await _report.GetMonthlyReportAsync(year, month);
        return Ok(report);
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateTime? date)
    {
        var bytes = await _report.ExportCsvAsync(date);
        return File(bytes, "text/csv", "usage_report.csv");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] DateTime? date)
    {
        var bytes = await _report.ExportPdfAsync(date);
        return File(bytes, "application/pdf", "usage_report.pdf");
    }

    [HttpGet("analytics/trends/daily")]
    public async Task<IActionResult> GetDailyTrends([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (from is null || to is null)
            return BadRequest("Both 'from' and 'to' are required.");

        var report = await _analytics.GetDailyTrendsAsync(from.Value, to.Value);
        return Ok(report);
    }

    [HttpGet("analytics/trends/monthly")]
    public async Task<IActionResult> GetMonthlyTrends([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        var report = await _analytics.GetMonthlyTrendsAsync(targetYear);
        return Ok(report);
    }

    [HttpGet("analytics/peaks")]
    public async Task<IActionResult> GetPeakUsage(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
    {
        if (from is null || to is null)
            return BadRequest("Both 'from' and 'to' are required.");

        var peak = await _analytics.GetPeakUsageAsync(from.Value, to.Value);
        return Ok(peak);
    }

    [HttpGet("analytics/anomalies")]
    public async Task<IActionResult> DetectAnomalies(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string mode = "z-score")
    {
        if (from is null || to is null)
            return BadRequest("Both 'from' and 'to' are required.");

        var result = await _analytics.DetectAnomaliesAsync(from.Value, to.Value, mode);
        return Ok(result);
    }

    [HttpGet("analytics/visualization")]
    public async Task<IActionResult> GetVisualizationData(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (from is null || to is null)
            return BadRequest("Both 'from' and 'to' are required.");

        var chart = await _analytics.GetVisualizationDataAsync(from.Value, to.Value);
        return Ok(chart);
    }
}