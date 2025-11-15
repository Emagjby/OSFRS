using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UsageAggregateDto
{
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = null!;

    [Required]
    public int Count { get; set; }

    [Required]
    public DateTime PeriodStart { get; set; }

    [Required]
    public DateTime PeriodEnd { get; set; }

    public int? UserId { get; set; }
    public int? FacilityId { get; set; }
}