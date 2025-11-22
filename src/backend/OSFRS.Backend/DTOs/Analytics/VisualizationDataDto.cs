namespace OSFRS.Backend.DTOs.Analytics;

/// <summary>
/// Represents a lightweight dataset formatted for frontend visualization components.
/// This structure is used to transmit chart-ready values to UI clients without exposing
/// internal analytics models.
/// </summary>
public record VisualizationDataDto
{
    /// <summary>
    /// The ordered set of labels associated with the dataset.
    /// These typically map to time intervals or category names on the X-axis.
    /// </summary>
    public IEnumerable<string> Labels { get; init; } = [];

    /// <summary>
    /// The numerical values corresponding to each label.
    /// These are typically plotted on the Y-axis of charts.
    /// </summary>
    public IEnumerable<int> Values { get; init; } = [];

    /// <summary>
    /// Hint provided to the frontend regarding the preferred chart type.
    /// Defaults to "line", but clients may choose to override this.
    /// </summary>
    public string ChartType { get; init; } = "line";
}