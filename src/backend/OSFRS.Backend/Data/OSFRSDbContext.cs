using Microsoft.EntityFrameworkCore;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Data;

public class OSFRSDbContext : DbContext
{
    public OSFRSDbContext(DbContextOptions<OSFRSDbContext> options) : base(options) { }

    //DbSets 
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

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
    }
}