using OSFRS.Backend.DTOs;
using OSFRS.Backend.Helpers.Reports;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;
    private readonly IAppLogger<ReportService> _logger;

    public ReportService(IReportRepository repo, IAppLogger<ReportService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    private async Task<(IEnumerable<UsageRecord> daily, IEnumerable<UsageRecord> monthly)>
    GetAggregatesForDateAsync(DateTime? date = null)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var daily = await _repo.GetDailyAggregatesAsync(targetDate);
        var monthly = await _repo.GetMonthlyAggregatesAsync(targetDate.Year, targetDate.Month);

        return (daily, monthly);
    }

    public async Task<byte[]> ExportCsvAsync(DateTime? date = null)
    {
        var (daily, monthly) = await GetAggregatesForDateAsync(date);
        var report = ReportFormatter.FormatAggregates(daily, monthly);

        _logger.LogInformation("Exporting CSV report...");
        return ReportFormatter.ToCsv(report);
    }

    public async Task<byte[]> ExportPdfAsync(DateTime? date = null)
    {
        var (daily, monthly) = await GetAggregatesForDateAsync(date);
        var report = ReportFormatter.FormatAggregates(daily, monthly);

        _logger.LogInformation("Exporting PDF report...");
        return ReportFormatter.ToPdf(report);
    }

    public async Task<ReportResultDto> GetDailyReportAsync(DateTime? dateTime = null)
    {
        var targetDate = dateTime?.Date ?? DateTime.UtcNow.Date;

        _logger.LogInformation("Generating DAILY report for {Date}", targetDate);

        var aggregates = await _repo.GetDailyAggregatesAsync(targetDate);

        return ReportFormatter.FormatAggregates(daily: aggregates, monthly: Enumerable.Empty<UsageRecord>());
    }

    public async Task<ReportResultDto> GetMonthlyReportAsync(int? year = null, int? month = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Date.Year;
        var targetMonth = month ?? DateTime.UtcNow.Date.Month;

        _logger.LogInformation("Generating MONTHLY report for {Month}/{Year}", targetMonth, targetYear);

        var aggregates = await _repo.GetMonthlyAggregatesAsync(targetYear, targetMonth);

        return ReportFormatter.FormatAggregates(monthly: aggregates, daily: Enumerable.Empty<UsageRecord>());
    }
}