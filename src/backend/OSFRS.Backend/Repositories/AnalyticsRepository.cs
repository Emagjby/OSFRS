using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Repository providing analytics-focused read operations over usage records,
/// including daily counts, monthly counts, and raw unaggregated event streams.
/// </summary>
public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<AnalyticsRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    /// <param name="logger">The application logger instance.</param>
    public AnalyticsRepository(OSFRSDbContext context, IAppLogger<AnalyticsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Computes the number of usage events per day within a specified UTC range.
    /// </summary>
    /// <param name="from">The inclusive start of the date range (UTC).</param>
    /// <param name="to">The inclusive end of the date range (UTC).</param>
    /// <returns>A collection of daily trend points.</returns>
    public async Task<IEnumerable<TrendPointDto>> GetDailyCountsAsync(DateTime from, DateTime to)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

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

    /// <summary>
    /// Computes the number of usage events per month for a given year.
    /// </summary>
    /// <param name="year">The target year (UTC).</param>
    /// <returns>A collection of monthly trend points.</returns>
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

    /// <summary>
    /// Retrieves all raw usage events within a specified timestamp range.
    /// Does not perform grouping, aggregation, or transformation.
    /// </summary>
    /// <param name="from">The start of the range (UTC).</param>
    /// <param name="to">The end of the range (UTC).</param>
    /// <returns>A list of raw usage records.</returns>
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