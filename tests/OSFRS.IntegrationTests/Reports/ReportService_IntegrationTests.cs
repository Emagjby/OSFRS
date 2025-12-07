using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Reports;

public class ReportService_IntegrationTests : IntegrationTestBase
{
    private IReportService Service => Factory.ReportService();
    private IReportRepository Repo => Factory.ReportRepo();
    private IUsageRepository Usage => Factory.UsageRepo();

    public ReportService_IntegrationTests()
        : base("OSFRS_IT_Reports_Service") { }

    private async Task SeedDailyAgg(DateTime ts)
    {
        await Usage.AddAsync(new UsageRecord { EventType = "X_DailyAggregate", Timestamp = ts });
        await Usage.SaveChangesAsync();
    }

    private async Task SeedMonthlyAgg(DateTime ts)
    {
        await Usage.AddAsync(new UsageRecord { EventType = "X_MonthlyAggregate", Timestamp = ts });
        await Usage.SaveChangesAsync();
    }

    // =============================================================
    // DAILY REPORT
    // =============================================================
    [Fact]
    public async Task DailyReport_ShouldReturnDailyAggregates()
    {
        var day = DateTime.UtcNow.Date;

        await SeedDailyAgg(day);
        await SeedDailyAgg(day);

        var rep = await Service.GetDailyReportAsync(day);

        rep.Daily.Should().HaveCount(2);
        rep.Monthly.Should().BeEmpty();
        rep.GeneratedAtUtc.Should().NotBe(default);
    }

    [Fact]
    public async Task DailyReport_ShouldReturnEmpty_WhenNone()
    {
        var day = DateTime.UtcNow.Date.AddDays(-50);

        var rep = await Service.GetDailyReportAsync(day);

        rep.Daily.Should().BeEmpty();
    }

    // =============================================================
    // MONTHLY REPORT
    // =============================================================
    [Fact]
    public async Task MonthlyReport_ShouldReturnMonthlyAggregates()
    {
        var now = DateTime.UtcNow;
        var ts = new DateTime(now.Year, now.Month, 5);

        await SeedMonthlyAgg(ts);
        await SeedMonthlyAgg(ts);

        var rep = await Service.GetMonthlyReportAsync(now.Year, now.Month);

        rep.Monthly.Should().HaveCount(2);
        rep.Daily.Should().BeEmpty();
        rep.GeneratedAtUtc.Should().NotBe(default);
    }

    [Fact]
    public async Task MonthlyReport_ShouldBeEmpty_WhenNone()
    {
        var rep = await Service.GetMonthlyReportAsync(2150, 1);
        rep.Monthly.Should().BeEmpty();
    }

    // =============================================================
    // CSV EXPORT
    // =============================================================
    [Fact]
    public async Task ExportCsv_ShouldProduceOutput_WhenAggregatesExist()
    {
        var day = DateTime.UtcNow.Date;

        await SeedDailyAgg(day);
        await SeedMonthlyAgg(day);

        var bytes = await Service.ExportCsvAsync(day);

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(10); // always non-empty
    }

    [Fact]
    public async Task ExportCsv_ShouldStillProduce_WhenEmpty()
    {
        var bytes = await Service.ExportCsvAsync(DateTime.UtcNow.AddYears(5));

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }

    // =============================================================
    // PDF EXPORT
    // =============================================================
    [Fact]
    public async Task ExportPdf_ShouldGiveOutput()
    {
        var day = DateTime.UtcNow.Date;

        await SeedDailyAgg(day);

        var bytes = await Service.ExportPdfAsync(day);

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task ExportPdf_ShouldStillProduceEmptyReport()
    {
        var bytes = await Service.ExportPdfAsync(DateTime.UtcNow.AddYears(2));

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }
}
