using FluentAssertions;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _repo;
    private readonly Mock<IAppLogger<ReportService>> _logger;

    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _repo = MockFactories.ReportRepo();
        _logger = MockFactories.Logger<ReportService>();

        _service = new ReportService(_repo.Object, _logger.Object);
    }

    // ============================================================
    // DAILY REPORT
    // ============================================================

    [Fact]
    public async Task DailyReport_ShouldFetchDailyAggregates()
    {
        var date = DateTime.UtcNow.Date;
        var list = FakeData.SimpleRecords(3);

        _repo.Setup(r => r.GetDailyAggregatesAsync(date)).ReturnsAsync(list);

        var result = await _service.GetDailyReportAsync(date);

        _repo.Verify(r => r.GetDailyAggregatesAsync(date), Times.Once);
        result.Daily.Should().HaveCount(3);
        result.Monthly.Should().BeEmpty();
    }

    // ============================================================
    // MONTHLY REPORT
    // ============================================================

    [Fact]
    public async Task MonthlyReport_ShouldFetchMonthlyAggregates()
    {
        int YEAR = 2025;
        int MONTH = 12;

        var list = FakeData.SimpleRecords(4);

        _repo.Setup(r => r.GetMonthlyAggregatesAsync(YEAR, MONTH)).ReturnsAsync(list);

        var result = await _service.GetMonthlyReportAsync(YEAR, MONTH);

        _repo.Verify(r => r.GetMonthlyAggregatesAsync(YEAR, MONTH), Times.Once);
        result.Monthly.Should().HaveCount(4);
        result.Daily.Should().BeEmpty();
    }

    // ============================================================
    // CSV EXPORT
    // ============================================================

    [Fact]
    public async Task ExportCsv_ShouldReturnCsvBytes()
    {
        var daily = FakeData.SimpleRecords(2);
        var monthly = FakeData.SimpleRecords(3);
        var DATE = DateTime.UtcNow.Date;

        _repo.Setup(r => r.GetDailyAggregatesAsync(DATE)).ReturnsAsync(daily);
        _repo.Setup(r => r.GetMonthlyAggregatesAsync(DATE.Year, DATE.Month)).ReturnsAsync(monthly);

        var bytes = await _service.ExportCsvAsync(DATE);

        bytes.Should().NotBeNull().And.NotBeEmpty();

        _repo.Verify(r => r.GetDailyAggregatesAsync(DATE), Times.Once);
        _repo.Verify(r => r.GetMonthlyAggregatesAsync(DATE.Year, DATE.Month), Times.Once);
    }

    // ============================================================
    // PDF EXPORT
    // ============================================================

    [Fact]
    public async Task ExportPdf_ShouldReturnPdfBytes()
    {
        var daily = FakeData.SimpleRecords(1);
        var monthly = FakeData.SimpleRecords(1);
        var DATE = DateTime.UtcNow.Date;

        _repo.Setup(r => r.GetDailyAggregatesAsync(DATE)).ReturnsAsync(daily);
        _repo.Setup(r => r.GetMonthlyAggregatesAsync(DATE.Year, DATE.Month)).ReturnsAsync(monthly);

        var bytes = await _service.ExportPdfAsync(DATE);

        bytes.Should().NotBeNull().And.NotBeEmpty();

        _repo.Verify(r => r.GetDailyAggregatesAsync(DATE), Times.Once);
        _repo.Verify(r => r.GetMonthlyAggregatesAsync(DATE.Year, DATE.Month), Times.Once);
    }

    [Fact]
    public async Task DailyReport_ShouldReturnEmpty_WhenNoDailyRecords()
    {
        var DATE = DateTime.UtcNow.Date;

        _repo.Setup(r => r.GetDailyAggregatesAsync(DATE)).ReturnsAsync(Array.Empty<UsageRecord>());

        var result = await _service.GetDailyReportAsync(DATE);

        result.Daily.Should().BeEmpty();
        result.Monthly.Should().BeEmpty(); // explicitly empty for daily report
    }

    [Fact]
    public async Task ExportCsv_ShouldReturnBytes_WhenNoData()
    {
        var DATE = DateTime.UtcNow.Date;

        _repo.Setup(r => r.GetDailyAggregatesAsync(DATE)).ReturnsAsync(Array.Empty<UsageRecord>());
        _repo
            .Setup(r => r.GetMonthlyAggregatesAsync(DATE.Year, DATE.Month))
            .ReturnsAsync(Array.Empty<UsageRecord>());

        var bytes = await _service.ExportCsvAsync(DATE);

        bytes.Should().NotBeNull();
        bytes.Should().NotBeEmpty(); // CSV still has structure
    }

    [Fact]
    public async Task ExportPdf_ShouldReturnBytes_WhenNoData()
    {
        var DATE = DateTime.UtcNow.Date;

        _repo.Setup(r => r.GetDailyAggregatesAsync(DATE)).ReturnsAsync(Array.Empty<UsageRecord>());
        _repo
            .Setup(r => r.GetMonthlyAggregatesAsync(DATE.Year, DATE.Month))
            .ReturnsAsync(Array.Empty<UsageRecord>());

        var bytes = await _service.ExportPdfAsync(DATE);

        bytes.Should().NotBeNull();
        bytes.Should().NotBeEmpty();
    }
}
