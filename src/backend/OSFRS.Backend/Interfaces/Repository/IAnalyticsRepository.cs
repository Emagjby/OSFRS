using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

public interface IAnalyticsRepository
{
    Task<IEnumerable<TrendPointDto>> GetDailyCountsAsync(DateTime from, DateTime to);
    Task<IEnumerable<TrendPointDto>> GetMonthlyCountsAsync(int year);
    Task<IEnumerable<UsageRecord>> GetRawEventsAsync(DateTime from, DateTime to);
}