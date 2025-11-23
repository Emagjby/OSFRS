using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs.Facilities;

/// <summary>
/// Represents the data required to create a new facility in the system.
/// Used by administrators when adding facilities such as rooms, courts, or resources.
/// </summary>
public record CreateFacilityDto
{
    /// <summary>
    /// The name of the facility.
    /// Example: "Basketball Court A".
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The facility type.
    /// This may represent categories such as "Gym", "Hall", or custom system-defined types.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The maximum capacity of the facility.
    /// Must be a positive integer.
    /// </summary>
    public int Capacity { get; init; }

    /// <summary>
    /// The initial status of the facility.
    /// Defaults to "Available".
    /// </summary>
    public string Status { get; init; } = "Available";
}