using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Infrastructure;

public static class SeedHelper
{
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

    public static void AddUsers(OSFRSDbContext db, int count)
    {
        for (int i = 0; i < count; i++)
            db.Users.Add(UserBuilder.Create());

        db.SaveChanges();

        foreach (var entry in db.ChangeTracker.Entries())
            entry.State = EntityState.Detached;
    }

    public static void AddUsers(OSFRSDbContext db, params User[] users)
    {
        db.Users.AddRange(users);
        db.SaveChanges();

        foreach (var entry in db.ChangeTracker.Entries())
            entry.State = EntityState.Detached;
    }
}
