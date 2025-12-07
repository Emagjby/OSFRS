using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;

namespace OSFRS.IntegrationTests.TestUtils.Extensions;

public static class DbExtensions
{
    /// <summary>
    /// Detaches all tracked entities to avoid EF tracking issues between operations.
    /// </summary>
    public static void DetachAll(this OSFRSDbContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }

    /// <summary>
    /// Reloads an entity from the database ensuring fresh, untracked state.
    /// </summary>
    public static async Task<T?> ReloadAsync<T>(this OSFRSDbContext db, T entity)
        where T : class
    {
        var entry = db.Entry(entity);
        var key = entry
            .Metadata.FindPrimaryKey()
            ?.Properties?.Select(p => entry.Property(p.Name).CurrentValue)
            .ToArray();

        if (key is null || key.Length == 0)
            return null;

        db.DetachAll();

        return await db.Set<T>().FindAsync(key);
    }

    /// <summary>
    /// Shorthand for counting entities in a table.
    /// </summary>
    public static Task<int> CountAsync<T>(this OSFRSDbContext db)
        where T : class => db.Set<T>().CountAsync();

    /// <summary>
    /// Shortcut for finding an entity by ID using inference.
    /// </summary>
    public static Task<T?> FindAsync<T>(this OSFRSDbContext db, int id)
        where T : class => db.Set<T>().FindAsync(id).AsTask();

    /// <summary>
    /// Removes all rows of T (fast reset).
    /// </summary>
    public static async Task RemoveAllAsync<T>(this OSFRSDbContext db)
        where T : class
    {
        db.Set<T>().RemoveRange(db.Set<T>());
        await db.SaveChangesAsync();
        db.DetachAll();
    }
}
