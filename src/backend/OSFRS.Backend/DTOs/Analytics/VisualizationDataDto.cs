namespace OSFRS.Backend.DTOs;

public record VisualizationDataDto
{
    public IEnumerable<string> Labels { get; set; } = [];
    public IEnumerable<int> Values { get; set; } = [];
    public string ChartType { get; set; } = "line";
}