using System.Text;
using FluentAssertions;
using OSFRS.Backend.Helpers.Reports;
using OSFRS.Models.Entities;
using static OSFRS.UnitTests.TestUtils.HelperTestHelpers;

namespace OSFRS.UnitTests.Helpers.Reports;

public class ReportFormatterTests
{
    // ============================================
    // FORMAT AGGREGATES
    // ============================================

    [Fact]
    public void FormatAggregates_ShouldMapUsageRecordsCorrectly()
    {
        var daily = new[] { Rec("A", "X") };
        var monthly = new[] { Rec("B", "Y") };

        var result = ReportFormatter.FormatAggregates(daily, monthly);

        result.Daily.Should().ContainSingle();
        result.Monthly.Should().ContainSingle();

        result.Daily[0].EventType.Should().Be("A");
        result.Daily[0].Metadata.Should().Be("X");

        result.Monthly[0].EventType.Should().Be("B");
        result.Monthly[0].Metadata.Should().Be("Y");
    }

    [Fact]
    public void FormatAggregates_ShouldSetGeneratedAtUtc()
    {
        var before = DateTime.UtcNow;
        var result = ReportFormatter.FormatAggregates(
            Array.Empty<UsageRecord>(),
            Array.Empty<UsageRecord>()
        );
        var after = DateTime.UtcNow;

        result.GeneratedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    // ============================================
    // CSV
    // ============================================

    [Fact]
    public void ToCsv_ShouldIncludeExpectedSections()
    {
        var report = SampleReport();
        var csv = Encoding.UTF8.GetString(ReportFormatter.ToCsv(report));

        csv.Should().Contain("===DAILY===");
        csv.Should().Contain("===MONTHLY===");
        csv.Should().Contain("ReservationCreated");
        csv.Should().Contain("FacilityUpdated");
    }

    [Fact]
    public void ToCsv_ShouldContainGeneratedAtTimestamp()
    {
        var report = SampleReport();

        var csv = Encoding.UTF8.GetString(ReportFormatter.ToCsv(report));

        csv.Should().Contain(report.GeneratedAtUtc.ToString("O"));
    }

    // ============================================
    // PDF (plain text)
    // ============================================

    [Fact]
    public void ToPdf_ShouldContainTitle()
    {
        var report = SampleReport();
        var pdf = Encoding.UTF8.GetString(ReportFormatter.ToPdf(report));

        pdf.Should().Contain("OSFRS Usage Report");
        pdf.Should().Contain("===DAILY===");
        pdf.Should().Contain("===MONTHLY===");
    }

    [Fact]
    public void ToPdf_ShouldFormatDailyAndMonthlyEntries()
    {
        var report = SampleReport();
        var pdf = Encoding.UTF8.GetString(ReportFormatter.ToPdf(report));

        pdf.Should().Contain("ReservationCreated");
        pdf.Should().Contain("FacilityUpdated");

        pdf.Should().Contain(report.GeneratedAtUtc.ToString("O"));
    }
}
