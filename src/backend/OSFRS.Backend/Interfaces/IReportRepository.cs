using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<UsageRecord>> GetDailyAggregatesAsync(DateTime dayUtc);
    Task<IEnumerable<UsageRecord>> GetMonthlyAggregatesAsync(int year, int month);

    Task<Report> SaveReportAsync(Report report);
}