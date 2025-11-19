namespace OSFRS.Backend.DTOs.Analytics;

public record VisualizationDataDto
{
    public IEnumerable<string> Labels { get; init; } = [];
    public IEnumerable<int> Values { get; init; } = [];
    public string ChartType { get; init; } = "line";
}