using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize(Roles = "Admin")]
public class StatisticsController : ControllerBase
{
    private readonly IUsageService _usage;
    private readonly IReportService _report;
    private readonly IAnalyticsService _analytics;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsController"/>.
    /// Provides endpoints for reporting, analytics, and usage insights.
    /// </summary>
    /// <param name="usage">Service for usage event queries and aggregation.</param>
    /// <param name="report">Service for generating reports in various formats.</param>
    /// <param name="analytics">Service providing statistical insights and anomaly detection.</param>
    /// <param name="logger">Application logger.</param>
    public StatisticsController(IUsageService usage, IReportService report, IAnalyticsService analytics, IAppLogger<StatisticsController> logger)
    {
        _usage = usage;
        _report = report;
        _analytics = analytics;
    }

    /// <summary>
    /// Retrieves raw usage events filtered by type, user, facility, or date range.
    /// </summary>
    /// <returns>A collection of usage events.</returns>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? eventType,
        [FromQuery] int? userId,
        [FromQuery] int? facilityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var events = await _usage.GetEventsAsync(eventType, userId, facilityId, from, to);
        return Ok(events);
    }

    /// <summary>
    /// Retrieves aggregated usage statistics for a specific day.
    /// </summary>
    /// <param name="date">The date to aggregate. Defaults to today.</param>
    /// <returns>Daily usage aggregates.</returns>
    [HttpGet("aggregate/daily")]
    public async Task<IActionResult> GetDailyAggregate([FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var results = await _usage.GetDailyAggregateAsync(targetDate);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves monthly aggregated usage data for a specific month and year.
    /// </summary>
    /// <returns>Monthly usage aggregates.</returns>
    [HttpGet("aggregate/monthly")]
    public async Task<IActionResult> GetMonthlyAggregate([FromQuery] int? year, [FromQuery] int? month)
    {
        int targetYear = year ?? DateTime.UtcNow.Year;
        int targetMonth = month ?? DateTime.UtcNow.Month;

        var results = await _usage.GetMonthlyAggregateAsync(targetYear, targetMonth);
        return Ok(results);
    }

    /// <summary>
    /// Forces the system to run aggregation for both daily and monthly usage.
    /// </summary>
    /// <returns>Status message indicating aggregation completion.</returns>
    [HttpPost("aggregate/run")]
    public async Task<IActionResult> RunAggregation()
    {
        await _usage.AggregateAsync();
        return Ok(new { message = "Aggregation executed successfully." });
    }

    /// <summary>
    /// Generates a daily usage report.
    /// </summary>
    /// <param name="date">Optional date to generate the report for.</param>
    /// <returns>A structured daily report.</returns>
    [HttpGet("reports/daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
    {
        var report = await _report.GetDailyReportAsync(date);
        return Ok(report);
    }

    /// <summary>
    /// Generates a monthly usage report.
    /// </summary>
    /// <returns>A structured monthly report.</returns>
    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int? year, [FromQuery] int? month)
    {
        var report = await _report.GetMonthlyReportAsync(year, month);
        return Ok(report);
    }

    /// <summary>
    /// Exports a daily usage report in CSV format.
    /// </summary>
    /// <returns>A CSV file containing usage data.</returns>
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateTime? date)
    {
        var bytes = await _report.ExportCsvAsync(date);
        return File(bytes, "text/csv", "usage_report.csv");
    }

    /// <summary>
    /// Exports a daily usage report in PDF format.
    /// </summary>
    /// <returns>A PDF file containing usage data.</returns>
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] DateTime? date)
    {
        var bytes = await _report.ExportPdfAsync(date);
        return File(bytes, "application/pdf", "usage_report.pdf");
    }

    /// <summary>
    /// Retrieves daily usage trends between two dates.
    /// </summary>
    /// <returns>Daily trend analysis data.</returns>
    [HttpGet("analytics/trends/daily")]
    public async Task<IActionResult> GetDailyTrends([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (from is null || to is null)
            return BadRequest("Both 'from' and 'to' are required.");

        var report = await _analytics.GetDailyTrendsAsync(from.Value, to.Value);
        return Ok(report);
    }

    /// <summary>
    /// Retrieves monthly usage trends for a given year.
    /// </summary>
    /// <returns>Monthly trend analysis.</returns>
    [HttpGet("analytics/trends/monthly")]
    public async Task<IActionResult> GetMonthlyTrends([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        var report = await _analytics.GetMonthlyTrendsAsync(targetYear);
        return Ok(report);
    }

    /// <summary>
    /// Identifies peak usage periods within a specific date range.
    /// </summary>
    /// <returns>A peak usage summary.</returns>
    [HttpGet("analytics/peaks")]
    public async Task<IActionResult> GetPeakUsage([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (from is null || to is null)
            return BadRequest("Both 'from' and 'to' are required.");

        var peak = await _analytics.GetPeakUsageAsync(from.Value, to.Value);
        return Ok(peak);
    }

    /// <summary>
    /// Performs anomaly detection on usage data within a date range.
    /// </summary>
    /// <param name="from">Start of the date range.</param>
    /// <param name="to">End of the date range.</param>
    /// <param name="mode">Detection mode ("z-score" or "mad").</param>
    /// <returns>A list of detected anomalies.</returns>
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

    /// <summary>
    /// Returns a dataset suitable for charts and visual dashboards.
    /// </summary>
    /// <returns>Visualization-ready usage analytics.</returns>
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