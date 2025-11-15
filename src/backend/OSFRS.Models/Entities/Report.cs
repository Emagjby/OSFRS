using System.ComponentModel.DataAnnotations;

namespace OSFRS.Models.Entities;

public class Report
{
    public int Id { get; set; } // Primary Key

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "Custom";
    // Daily, weekly, custom...

    [Required]
    public DateTime GeneratedAt { get; set; }

    public string? AggregatedData { get; set; }
}