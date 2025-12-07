using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.IntegrationTests.TestUtils.Builders;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Seed;

public static class SeedHelper
{
    // ------------------------------------------------------------
    // Generic Add
    // ------------------------------------------------------------
    public static async Task<T> AddAsync<T>(this OSFRSDbContext db, T entity)
        where T : class
    {
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public static async Task AddRangeAsync<T>(this OSFRSDbContext db, params T[] entities)
        where T : class
    {
        db.Set<T>().AddRange(entities);
        await db.SaveChangesAsync();
    }

    // ------------------------------------------------------------
    // Specific Quick-Seed Helpers
    // ------------------------------------------------------------
    public static async Task<User> AddUserAsync(this OSFRSDbContext db, User? user = null)
    {
        user ??= UserBuilder.Create().Build();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<Facility> AddFacilityAsync(
        this OSFRSDbContext db,
        Facility? facility = null
    )
    {
        facility ??= FacilityBuilder.Create().Build();
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();
        return facility;
    }

    public static async Task<MaintenanceRecord> AddMaintenanceAsync(
        this OSFRSDbContext db,
        MaintenanceRecord? record = null
    )
    {
        record ??= MaintenanceBuilder.Create().Build();
        db.MaintenanceRecords.Add(record);
        await db.SaveChangesAsync();
        return record;
    }

    public static async Task<Reservation> AddReservationAsync(
        this OSFRSDbContext db,
        Reservation? r = null
    )
    {
        r ??= ReservationBuilder.Create().Build();
        db.Reservations.Add(r);
        await db.SaveChangesAsync();
        return r;
    }

    public static async Task<UsageRecord> AddUsageAsync(
        this OSFRSDbContext db,
        UsageRecord? r = null
    )
    {
        r ??= UsageBuilder.Create().Build();
        db.UsageRecords.Add(r);
        await db.SaveChangesAsync();
        return r;
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------
    public static void DetachAll(this OSFRSDbContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }
}
