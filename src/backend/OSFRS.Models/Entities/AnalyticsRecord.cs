using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSFRS.Models.Entities;

[Table("Analytics")]
public class AnalyticsRecord
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;
    // examples: DailyUsage, PeakHour, etc.

    public int? UserId { get; set; }
    public int? FacilityId { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public string? AggregatedData { get; set; }
}