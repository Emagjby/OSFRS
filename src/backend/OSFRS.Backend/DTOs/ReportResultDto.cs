namespace OSFRS.Backend.DTOs;

public record ReportResultDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public List<ReportEntryDto> Daily { get; set; } = [];
    public List<ReportEntryDto> Monthly { get; set; } = [];
}