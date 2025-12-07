using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils.AssertHelpers;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class BaseRepositoryTests
{
    private readonly Mock<IAppLogger<BaseRepository<User>>> _logger = new();

    private UserRepository CreateRepo(TestDbContext db) =>
        new UserRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // ADD
    // ------------------------------------------------------------
    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create(name: "John");

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var fromDb = await db.Db.Users.FindAsync(user.Id);
        AssertUser.Equal(fromDb!, user);
    }

    // ------------------------------------------------------------
    // ADD RANGE
    // ------------------------------------------------------------
    [Fact]
    public async Task AddRangeAsync_ShouldAddEntities()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var users = new[] { UserBuilder.Create(name: "A"), UserBuilder.Create(name: "B") };

        await repo.AddRangeAsync(users);
        await repo.SaveChangesAsync();

        var all = await db.Db.Users.ToListAsync();
        all.Should().HaveCount(2);
    }

    // ------------------------------------------------------------
    // GET BY ID
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create();
        db.Db.Users.Add(user);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByIdAsync(user.Id);

        AssertUser.Equal(result!, user);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var result = await repo.GetByIdAsync(999);
        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // GET ALL
    // ------------------------------------------------------------
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        SeedHelper.AddUsers(db.Db, 3);

        var all = await repo.GetAllAsync();
        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllReadonlyAsync_ShouldReturnAllEntitiesWithoutTracking()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        SeedHelper.AddUsers(db.Db, 2);

        var all = await repo.GetAllReadonlyAsync();
        all.Should().HaveCount(2);
        db.Db.ChangeTracker.Entries().Should().BeEmpty();
    }

    // ------------------------------------------------------------
    // FIND
    // ------------------------------------------------------------
    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        SeedHelper.AddUsers(
            db.Db,
            UserBuilder.Create(email: "a@mail.com"),
            UserBuilder.Create(email: "b@mail.com")
        );

        var result = await repo.FindAsync(x => x.Email == "a@mail.com");

        result.Should().HaveCount(1);
        result.First().Email.Should().Be("a@mail.com");
    }

    // ------------------------------------------------------------
    // EXISTS
    // ------------------------------------------------------------
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        SeedHelper.AddUsers(db.Db, 1);
        var id = db.Db.Users.First().Id;

        var exists = await repo.ExistsAsync(id);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var exists = await repo.ExistsAsync(999);
        exists.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // REMOVE
    // ------------------------------------------------------------
    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create();
        db.Db.Users.Add(user);
        await db.Db.SaveChangesAsync();

        repo.Remove(user);
        await repo.SaveChangesAsync();

        var exists = await db.Db.Users.FindAsync(user.Id);
        exists.Should().BeNull();
    }

    // ------------------------------------------------------------
    // SAVE CHANGES
    // ------------------------------------------------------------
    [Fact]
    public async Task SaveChangesAsync_ShouldCommitChanges()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create();
        await repo.AddAsync(user);

        var saved = await repo.SaveChangesAsync();

        saved.Should().BeGreaterThan(0);
        (await db.Db.Users.CountAsync()).Should().Be(1);
    }
}
