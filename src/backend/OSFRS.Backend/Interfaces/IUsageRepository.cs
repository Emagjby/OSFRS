using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IUsageRepository
{
    Task<UsageRecord> AddAsync(UsageRecord usageRecord);
    Task AddRangeAsync(IEnumerable<UsageRecord> usageRecords);

    Task<IEnumerable<UsageRecord>> QueryAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    );

    Task<IEnumerable<UsageRecord>> AggregateDailyAsync(DateTime date);
    Task<IEnumerable<UsageRecord>> AggregateMonthlyAsync(int year, int month);

    Task<IEnumerable<UsageRecord>> GetDailyAnalyticsAsync(DateTime date);
    Task<IEnumerable<UsageRecord>> GetMonthlyAnalyticsAsync(int year, int month);

    Task SaveChangesAsync();
}