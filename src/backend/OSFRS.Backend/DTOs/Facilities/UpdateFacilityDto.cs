namespace OSFRS.Backend.DTOs.Facilities;

/// <summary>
/// Represents a partial update request for an existing facility.
/// All fields are optional; only provided values will be updated.
/// </summary>
public record UpdateFacilityDto
{
    /// <summary>
    /// The new name of the facility.
    /// If null, the existing name remains unchanged.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The updated type or category of the facility.
    /// If null, the existing type remains unchanged.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// The updated capacity value.
    /// Must be greater than zero if provided.
    /// </summary>
    public int? Capacity { get; init; }

    /// <summary>
    /// The updated operational status of the facility.
    /// Valid options include "Available", "Unavailable", and "UnderMaintenance".
    /// </summary>
    public string? Status { get; init; }
}