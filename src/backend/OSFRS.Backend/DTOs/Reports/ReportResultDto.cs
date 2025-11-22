namespace OSFRS.Backend.DTOs.Reports;

/// <summary>
/// Represents the result of a generated usage report, including both
/// daily and monthly aggregated entries.
/// </summary>
public record ReportResultDto
{
    /// <summary>
    /// The UTC timestamp indicating when the report was generated.
    /// </summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>
    /// A collection of daily report entries containing event-level details.
    /// </summary>
    public List<ReportEntryDto> Daily { get; init; } = [];

    /// <summary>
    /// A collection of monthly report entries containing event-level details.
    /// </summary>
    public List<ReportEntryDto> Monthly { get; init; } = [];
}