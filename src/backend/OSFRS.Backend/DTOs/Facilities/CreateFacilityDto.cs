namespace OSFRS.Backend.DTOs.Facilities;

public record CreateFacilityDto
{
    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public string Status { get; init; } = "Available";
}