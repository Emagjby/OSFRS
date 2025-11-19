using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UpdateFacilityDto
{
    [MaxLength(100)]
    public string? Name { get; init; }

    [MaxLength(50)]
    public string? Type { get; init; }

    [Range(1, 1000)]
    public int? Capacity { get; init; }

    [MaxLength(20)]
    public string? Status { get; init; }
}