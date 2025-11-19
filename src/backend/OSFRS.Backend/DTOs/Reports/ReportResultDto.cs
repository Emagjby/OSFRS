namespace OSFRS.Backend.DTOs.Reports;

public record ReportResultDto
{
    public DateTime GeneratedAtUtc { get; init; }
    public List<ReportEntryDto> Daily { get; init; } = [];
    public List<ReportEntryDto> Monthly { get; init; } = [];
}