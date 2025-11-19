using System.Text;
using OSFRS.Backend.DTOs.Reports;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Helpers.Reports;

public static class ReportFormatter
{
    public static byte[] ToCsv(ReportResultDto report)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"GeneratedAt,{report.GeneratedAtUtc:O}");
        sb.AppendLine();
        sb.AppendLine("===DAILY===");

        foreach (var d in report.Daily)
            sb.AppendLine($"{d.EventType},{d.Timestamp:O},{d.Metadata}");

        sb.AppendLine("===MONTHLY===");
        foreach (var m in report.Monthly)
            sb.AppendLine($"{m.EventType},{m.Timestamp:O},{m.Metadata}");

        var csv = sb.ToString();

        return Encoding.UTF8.GetBytes(csv);
    }

    public static byte[] ToPdf(ReportResultDto report)
    {
        var plainText =
            $"OSFRS Usage Report\nGenerated: {report.GeneratedAtUtc:O}\n\n" +
            $"===DAILY===\n" +
            string.Join("\n", report.Daily.Select(r => $"{r.EventType} @ {r.Timestamp:O} -> {r.Metadata}")) +
            $"\n\n===MONTHLY===\n" +
            string.Join("\n", report.Monthly.Select(r => $"{r.EventType} @ {r.Timestamp:O} -> {r.Metadata}"));

        return Encoding.UTF8.GetBytes(plainText);

    }

    public static ReportResultDto FormatAggregates(
        IEnumerable<UsageRecord> daily,
        IEnumerable<UsageRecord> monthly
    ) => new()
    {
        GeneratedAtUtc = DateTime.UtcNow,
        Daily = daily.Select(ToEntry).ToList(),
        Monthly = monthly.Select(ToEntry).ToList()
    };

    private static ReportEntryDto ToEntry(UsageRecord usageRecord) => new()
    {
        EventType = usageRecord.EventType,
        Timestamp = usageRecord.Timestamp,
        Metadata = usageRecord.AggregatedData
    };
}