using OSFRS.Backend.DTOs.Reports;
using OSFRS.Backend.Helpers.Reports;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides reporting operations for usage analytics, including generation
/// of daily and monthly reports as well as export functionality for CSV and PDF.
/// </summary>
public class ReportService : IReportService
{
    private readonly IReportRepository _repo;
    private readonly IAppLogger<ReportService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportService"/> class.
    /// </summary>
    /// <param name="repo">Repository used to fetch usage aggregates.</param>
    /// <param name="logger">Logging abstraction for report operations.</param>
    public ReportService(IReportRepository repo, IAppLogger<ReportService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves both daily and monthly aggregates for a specified date.
    /// </summary>
    /// <param name="date">Optional date for which aggregates are fetched. Defaults to today.</param>
    /// <returns>A tuple containing daily and monthly usage records.</returns>
    private async Task<(IEnumerable<UsageRecord> daily, IEnumerable<UsageRecord> monthly)>
        GetAggregatesForDateAsync(DateTime? date = null)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var daily = await _repo.GetDailyAggregatesAsync(targetDate);
        var monthly = await _repo.GetMonthlyAggregatesAsync(targetDate.Year, targetDate.Month);

        return (daily, monthly);
    }

    /// <summary>
    /// Exports usage aggregates into a CSV-formatted byte array.
    /// </summary>
    /// <param name="date">Optional date for which the CSV is generated.</param>
    /// <returns>A UTF-8 encoded CSV file as a byte array.</returns>
    public async Task<byte[]> ExportCsvAsync(DateTime? date = null)
    {
        var (daily, monthly) = await GetAggregatesForDateAsync(date);
        var report = ReportFormatter.FormatAggregates(daily, monthly);

        _logger.LogInformation("Exporting CSV report...");
        return ReportFormatter.ToCsv(report);
    }

    /// <summary>
    /// Exports usage aggregates into a PDF-formatted byte array.
    /// </summary>
    /// <param name="date">Optional date for which the PDF is generated.</param>
    /// <returns>A plain-text PDF document encoded as a byte array.</returns>
    public async Task<byte[]> ExportPdfAsync(DateTime? date = null)
    {
        var (daily, monthly) = await GetAggregatesForDateAsync(date);
        var report = ReportFormatter.FormatAggregates(daily, monthly);

        _logger.LogInformation("Exporting PDF report...");
        return ReportFormatter.ToPdf(report);
    }

    /// <summary>
    /// Generates a daily report for the specified date.
    /// </summary>
    /// <param name="dateTime">The target date. Defaults to today.</param>
    /// <returns>A <see cref="ReportResultDto"/> containing daily aggregates.</returns>
    public async Task<ReportResultDto> GetDailyReportAsync(DateTime? dateTime = null)
    {
        var targetDate = dateTime?.Date ?? DateTime.UtcNow.Date;

        _logger.LogInformation("Generating DAILY report for {Date}", targetDate);

        var aggregates = await _repo.GetDailyAggregatesAsync(targetDate);

        return ReportFormatter.FormatAggregates(
            daily: aggregates,
            monthly: Enumerable.Empty<UsageRecord>()
        );
    }

    /// <summary>
    /// Generates a monthly report for the specified year and month.
    /// </summary>
    /// <param name="year">Target year. Defaults to the current year.</param>
    /// <param name="month">Target month. Defaults to the current month.</param>
    /// <returns>A <see cref="ReportResultDto"/> containing monthly aggregates.</returns>
    public async Task<ReportResultDto> GetMonthlyReportAsync(int? year = null, int? month = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        _logger.LogInformation(
            "Generating MONTHLY report for {Month}/{Year}",
            targetMonth,
            targetYear
        );

        var aggregates = await _repo.GetMonthlyAggregatesAsync(targetYear, targetMonth);

        return ReportFormatter.FormatAggregates(
            monthly: aggregates,
            daily: Enumerable.Empty<UsageRecord>()
        );
    }
}