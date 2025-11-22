namespace OSFRS.Backend.DTOs.Facilities;

/// <summary>
/// Represents a facility as exposed through API responses.
/// Used for listing, viewing, and managing facilities within the system.
/// </summary>
public record FacilityDto
{
    /// <summary>
    /// The unique identifier of the facility.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The display name of the facility.
    /// Example: "GymFacility B".
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The type or category of the facility.
    /// Example: "Gym", "Court", "Field".
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The maximum number of users or participants the facility can support.
    /// </summary>
    public int Capacity { get; init; }

    /// <summary>
    /// The current operational status of the facility.
    /// Valid values include "Available", "Unavailable", and "UnderMaintenance".
    /// </summary>
    public string Status { get; init; } = string.Empty;
}