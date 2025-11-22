using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Repository for accessing and persisting report-related data, including
/// daily and monthly aggregated usage metrics.
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<ReportRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportRepository"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework database context.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public ReportRepository(OSFRSDbContext context, IAppLogger<ReportRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all daily aggregated usage records for the specified date.
    /// </summary>
    /// <param name="dayUtc">The target date in UTC.</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> entries representing daily aggregates.
    /// </returns>
    public async Task<IEnumerable<UsageRecord>> GetDailyAggregatesAsync(DateTime dayUtc)
    {
        var start = dayUtc.Date;
        var end = start.AddDays(1);

        var records = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= start &&
                r.Timestamp < end &&
                r.EventType.Contains("DailyAggregate"))
            .ToListAsync();

        _logger.LogInformation(
            "Fetched {Count} daily aggregates for {Date}",
            records.Count,
            start.ToShortDateString()
        );

        return records;
    }

    /// <summary>
    /// Retrieves all monthly aggregated usage records for the specified year and month.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <param name="month">The target month.</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> entries representing monthly aggregates.
    /// </returns>
    public async Task<IEnumerable<UsageRecord>> GetMonthlyAggregatesAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var records = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= monthStart &&
                r.Timestamp < monthEnd &&
                r.EventType.Contains("MonthlyAggregate"))
            .ToListAsync();

        _logger.LogInformation(
            "Fetched {Count} monthly aggregates for {Year}-{Month}",
            records.Count,
            year,
            month
        );

        return records;
    }

    /// <summary>
    /// Persists a generated <see cref="Report"/> to the database.
    /// </summary>
    /// <param name="report">The report entity to save.</param>
    /// <returns>
    /// The persisted <see cref="Report"/> instance with updated identifiers.
    /// </returns>
    public async Task<Report> SaveReportAsync(Report report)
    {
        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Report saved: {Name}",
            report.Name
        );

        return report;
    }
}