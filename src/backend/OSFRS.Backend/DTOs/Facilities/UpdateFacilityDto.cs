namespace OSFRS.Backend.DTOs.Facilities;

public record UpdateFacilityDto
{
    public string? Name { get; init; }

    public string? Type { get; init; }

    public int? Capacity { get; init; }

    public string? Status { get; init; }
}