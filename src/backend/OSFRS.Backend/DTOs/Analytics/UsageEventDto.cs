using System.ComponentModel.DataAnnotations;

namespace OSFRS.Backend.DTOs;

public record UsageEventDto
{
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = null!;

    public int? UserId { get; set; }
    public int? FacilityId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Dictionary<string, string>? Metadata { get; set; }
}