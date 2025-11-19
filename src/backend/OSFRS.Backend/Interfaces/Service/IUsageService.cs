using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Service;

public interface IUsageService
{
    Task LogEventAsync(UsageEventDto dto);

    Task<IEnumerable<UsageRecord>> GetEventsAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    );

    Task BulkLogAsync(IEnumerable<UsageEventDto> dtos);

    Task AggregateAsync();

    Task<IEnumerable<UsageRecord>> GetDailyAggregateAsync(DateTime date);
    Task<IEnumerable<UsageRecord>> GetMonthlyAggregateAsync(int year, int month);
}