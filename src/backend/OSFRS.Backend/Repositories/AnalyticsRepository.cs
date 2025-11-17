using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<AnalyticsRepository> _logger;

    public AnalyticsRepository(OSFRSDbContext context, IAppLogger<AnalyticsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TrendPointDto>> GetDailyCountsAsync(DateTime from, DateTime to)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to   = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var results = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= from &&
                r.Timestamp <= to)
            .GroupBy(g => g.Timestamp.ToUniversalTime().Date)
            .Select(g => new TrendPointDto
            {
                Timestamp = g.Key,
                Count = g.Count()
            })
            .OrderBy(tp => tp.Timestamp)
            .ToListAsync();

        _logger.LogInformation("Daily trend query returned {Count} points", results.Count);

        return results;
    }

    public async Task<IEnumerable<TrendPointDto>> GetMonthlyCountsAsync(int year)
    {
        var results = await _context.UsageRecords
            .Where(r => r.Timestamp.Year == year)
            .GroupBy(g => new { g.Timestamp.ToUniversalTime().Year, g.Timestamp.ToUniversalTime().Month })
            .Select(g => new TrendPointDto
            {
                Timestamp = new DateTime(g.Key.Year, g.Key.Month, 1),
                Count = g.Count()
            })
            .OrderBy(tp => tp.Timestamp)
            .ToListAsync();

        _logger.LogInformation("Monthly trend query returned {Count} points", results.Count);

        return results;
    }

    public async Task<IEnumerable<UsageRecord>> GetRawEventsAsync(DateTime from, DateTime to)
    {
        var records = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= from &&
                r.Timestamp <= to)
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Raw event query returned {Count} rows", records.Count);

        return records;
    }
}