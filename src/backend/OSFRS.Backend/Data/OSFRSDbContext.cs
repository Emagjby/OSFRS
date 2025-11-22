using Microsoft.EntityFrameworkCore;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Data;

/// <summary>
/// Represents the primary Entity Framework Core database context for the OSFRS backend.
/// Manages entity sets, relationships, constraints, and schema configuration.
/// </summary>
public class OSFRSDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OSFRSDbContext"/> class
    /// using the provided database options.
    /// </summary>
    /// <param name="options">The database configuration options.</param>
    public OSFRSDbContext(DbContextOptions<OSFRSDbContext> options) : base(options) { }

    // ------------------------------------------------------------------------
    // DbSets
    // ------------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the users registered in the system.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets reservation records for facility bookings.
    /// </summary>
    public DbSet<Reservation> Reservations { get; set; }

    /// <summary>
    /// Gets or sets the facilities available for reservation.
    /// </summary>
    public DbSet<Facility> Facilities { get; set; }

    /// <summary>
    /// Gets or sets maintenance records for facilities.
    /// </summary>
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

    /// <summary>
    /// Gets or sets usage log entries for auditing and analytics.
    /// </summary>
    public DbSet<UsageRecord> UsageRecords { get; set; }

    /// <summary>
    /// Gets or sets generated system reports.
    /// </summary>
    public DbSet<Report> Reports { get; set; }

    /// <summary>
    /// Gets or sets aggregated analytics data used for trends, peaks, and anomaly detection.
    /// </summary>
    public DbSet<AnalyticsRecord> Analytics { get; set; }

    // ------------------------------------------------------------------------
    // Model configuration
    // ------------------------------------------------------------------------

    /// <summary>
    /// Configures database schema, entity relationships, indexes, and constraints.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // USER
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // RESERVATIONS - USER
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany() // no back-reference on User
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // RESERVATIONS - FACILITY
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Facility)
            .WithMany(f => f.Reservations)
            .HasForeignKey(r => r.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // MAINTENANCE - FACILITY
        modelBuilder.Entity<MaintenanceRecord>()
            .HasOne(m => m.Facility)
            .WithMany(f => f.MaintenanceRecords)
            .HasForeignKey(m => m.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // USAGE RECORDS
        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Facility)
                .WithMany()
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // REPORTS
        modelBuilder.Entity<Report>(entity =>
        {
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.AggregatedData)
                .IsRequired();
        });

        // ANALYTICS
        modelBuilder.Entity<AnalyticsRecord>(entity =>
        {
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.AggregatedData)
                .IsRequired();
        });
    }
}