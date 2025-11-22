using System.Text;
using OSFRS.Backend.DTOs.Reports;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Helpers.Reports;

/// <summary>
/// Provides utility methods for converting aggregated usage reports
/// into CSV, PDF-like text, and normalized DTO formats.
/// </summary>
public static class ReportFormatter
{
    /// <summary>
    /// Converts a <see cref="ReportResultDto"/> into a UTF-8 encoded CSV file.
    /// </summary>
    /// <param name="report">The aggregated report result.</param>
    /// <returns>A byte array containing the CSV representation.</returns>
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

    /// <summary>
    /// Converts a <see cref="ReportResultDto"/> into a UTF-8 encoded
    /// plain-text PDF-like document. This is not a real PDF format, but
    /// provides human-readable content suitable for export.
    /// </summary>
    /// <param name="report">The aggregated report result.</param>
    /// <returns>A byte array containing the formatted text.</returns>
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

    /// <summary>
    /// Converts raw daily and monthly <see cref="UsageRecord"/> sequences
    /// into a normalized <see cref="ReportResultDto"/> structure.
    /// </summary>
    /// <param name="daily">Daily usage records.</param>
    /// <param name="monthly">Monthly usage records.</param>
    /// <returns>A populated <see cref="ReportResultDto"/>.</returns>
    public static ReportResultDto FormatAggregates(
        IEnumerable<UsageRecord> daily,
        IEnumerable<UsageRecord> monthly
    ) => new()
    {
        GeneratedAtUtc = DateTime.UtcNow,
        Daily = daily.Select(ToEntry).ToList(),
        Monthly = monthly.Select(ToEntry).ToList()
    };

    /// <summary>
    /// Converts a <see cref="UsageRecord"/> into a <see cref="ReportEntryDto"/>.
    /// </summary>
    /// <param name="usageRecord">The source usage record.</param>
    /// <returns>The mapped report entry.</returns>
    private static ReportEntryDto ToEntry(UsageRecord usageRecord) => new()
    {
        EventType = usageRecord.EventType,
        Timestamp = usageRecord.Timestamp,
        Metadata = usageRecord.AggregatedData
    };
}