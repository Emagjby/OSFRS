using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class UsageRepository : IUsageRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<UsageRepository> _logger;

    public UsageRepository(OSFRSDbContext context, IAppLogger<UsageRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UsageRecord> AddAsync(UsageRecord usageRecord)
    {
        await _context.UsageRecords.AddAsync(usageRecord);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Usage event logged: {EventType} (User={UserId}, Facility={FacilityId})",
            usageRecord.EventType,
            usageRecord.UserId!,
            usageRecord.FacilityId!
        );

        return usageRecord;
    }

    public async Task AddRangeAsync(IEnumerable<UsageRecord> usageRecords)
    {
        await _context.UsageRecords.AddRangeAsync(usageRecords);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Bulk usage logging: {Count} entries added.",
            usageRecords.Count()
        );
    }
    
    private async Task<bool> HasDailyAggregateAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var usageRecords = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= dayStart &&
                r.Timestamp < dayEnd &&
                r.EventType.Contains("DailyAggregate"))
            .ToListAsync();

        return usageRecords.Count != 0;
    }

    private async Task<IEnumerable<UsageRecord>> CreateDailyAggregateAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var usageRecords = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= dayStart &&
                r.Timestamp < dayEnd &&
                !r.EventType.Contains("MonthlyAggregate") &&
                !r.EventType.Contains("DailyAggregate"))
            .ToListAsync();

        if (!usageRecords.Any())
        {
            _logger.LogWarning(
                "No usage records found for daily aggregation on {Date}",
                dayStart
            );
            return Enumerable.Empty<UsageRecord>();
        }

        var aggregated = usageRecords
            .GroupBy(r => new { r.EventType, r.UserId, r.FacilityId })
            .Select(g => new UsageRecord
            {
                EventType = $"{g.Key.EventType}_DailyAggregate",
                UserId = g.Key.UserId,
                FacilityId = g.Key.FacilityId,
                Timestamp = dayStart,
                AggregatedData = $"Count={g.Count()}"
            })
            .ToList();

        await _context.UsageRecords.AddRangeAsync(aggregated);
        await _context.SaveChangesAsync();

        return aggregated;
    }

    public async Task DeleteDailyAggregateAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var usageRecords = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= dayStart &&
                r.Timestamp < dayEnd &&
                r.EventType.Contains("DailyAggregate"))
            .ToListAsync();

        _context.UsageRecords.RemoveRange(usageRecords);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UsageRecord>> AggregateDailyAsync(DateTime date)
    {
        if (await HasDailyAggregateAsync(date))
        {
            await DeleteDailyAggregateAsync(date);
            _logger.LogInformation("Deleted existing daily aggregate for {date}", date);
        }
        
        _logger.LogInformation("Created daily aggregate for {date}", date);
        return await CreateDailyAggregateAsync(date);
    }

    private async Task<bool> HasMonthlyAggregateAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var usageRecords = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= monthStart &&
                r.Timestamp < monthEnd &&
                r.EventType.Contains("MonthlyAggregate"))
            .ToListAsync();

        return usageRecords.Count != 0;
    }

    private async Task<IEnumerable<UsageRecord>> CreateMonthlyAggregateAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var usageRecords = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= monthStart &&
                r.Timestamp < monthEnd &&
                !r.EventType.Contains("MonthlyAggregate") &&
                !r.EventType.Contains("DailyAggregate"))
            .ToListAsync();

        if (!usageRecords.Any())
        {
            _logger.LogWarning(
                "No usage records found for monthly aggregation: {Year}-{Month}",
                year,
                month
            );
            return Enumerable.Empty<UsageRecord>();
        }

        var aggregated = usageRecords
            .GroupBy(r => new { r.EventType, r.UserId, r.FacilityId })
            .Select(g => new UsageRecord
            {
                EventType = $"{g.Key.EventType}_MonthlyAggregate",
                UserId = g.Key.UserId,
                FacilityId = g.Key.FacilityId,
                Timestamp = monthStart,
                AggregatedData = $"Count={g.Count()}"
            })
            .ToList();

        await _context.UsageRecords.AddRangeAsync(aggregated);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Monthly aggregation completed: {Count} entries for {Year}-{Month}",
            aggregated.Count,
            year,
            month
        );

        return aggregated;
    }

    private async Task DeleteMonthlyAggregateAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var usageRecords = await _context.UsageRecords
            .Where(r =>
                r.Timestamp >= monthStart &&
                r.Timestamp < monthEnd &&
                r.EventType.Contains("MonthlyAggregate"))
            .ToListAsync();

        _context.UsageRecords.RemoveRange(usageRecords);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UsageRecord>> AggregateMonthlyAsync(int year, int month)
    {
        if (await HasMonthlyAggregateAsync(year, month))
        {
            await DeleteMonthlyAggregateAsync(year, month);
            _logger.LogInformation("Deleted existing monthly aggregate for {Year}/{Month}", year, month);
        }
        
        _logger.LogInformation("Created daily aggregate for {Year}/{Month}", year, month);
        return await CreateMonthlyAggregateAsync(year, month);
    }

    public async Task<IEnumerable<UsageRecord>> GetDailyAnalyticsAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        return await _context.UsageRecords
            .Where(x => x.Timestamp >= start && x.Timestamp < end)
            .ToListAsync();
    }

    public async Task<IEnumerable<UsageRecord>> GetMonthlyAnalyticsAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);

        return await _context.UsageRecords
            .Where(x => x.Timestamp >= start && x.Timestamp < end)
            .ToListAsync();
    }

    public async Task<IEnumerable<UsageRecord>> QueryAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null)
    {
        var query = _context.UsageRecords.AsQueryable();

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(u => u.EventType == eventType);

        if (userId.HasValue)
            query = query.Where(u => u.UserId == userId);

        if (facilityId.HasValue)
            query = query.Where(u => u.FacilityId == facilityId);

        if (start.HasValue)
            query = query.Where(u => u.Timestamp >= start);

        if (end.HasValue)
            query = query.Where(u => u.Timestamp <= end);

        return await query
            .OrderBy(u => u.Timestamp)
            .ToListAsync();
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    
}