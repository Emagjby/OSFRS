using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<ReportRepository> _logger;

    public ReportRepository(OSFRSDbContext context, IAppLogger<ReportRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

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