using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

public interface IUsageRepository : IBaseRepository<UsageRecord>
{

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
}