using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Data;

public class OSFRSDbContext : DbContext
{
    public OSFRSDbContext(DbContextOptions<OSFRSDbContext> options) : base(options) { }

    //DbSets 
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<UsageRecord> UsageRecords { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<AnalyticsRecord> Analytics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Facility)
            .WithMany()
            .HasForeignKey(r => r.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaintenanceRecord>()
            .HasOne(m => m.Facility)
            .WithMany()
            .HasForeignKey(m => m.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

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