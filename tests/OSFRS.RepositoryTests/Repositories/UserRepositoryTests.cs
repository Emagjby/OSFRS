using FluentAssertions;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils.AssertHelpers;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<IAppLogger<BaseRepository<User>>> _logger = new();

    private UserRepository CreateRepo(TestDbContext db) =>
        new UserRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // GET BY EMAIL
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByEmailAsync_ShouldReturnEntity_WhenExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create(email: "test@mail.com");
        await db.Db.AddAsync(user);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByEmailAsync("test@mail.com");

        AssertUser.Equal(result!, user);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var result = await repo.GetByEmailAsync("missing@mail.com");

        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // GET BY USERNAME
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnEntity_WhenExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create(username: "john123");
        await db.Db.AddAsync(user);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByUsernameAsync("john123");

        AssertUser.Equal(result!, user);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var result = await repo.GetByUsernameAsync("nope123");

        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // GET BY USERNAME OR EMAIL
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnByUsername()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create(username: "uniqueUser", email: "mail@mail.com");
        await db.Db.AddAsync(user);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByUsernameOrEmailAsync("uniqueUser");

        AssertUser.Equal(result!, user);
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnByEmail()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create(username: "xxx", email: "hello@mail.com");
        await db.Db.AddAsync(user);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByUsernameOrEmailAsync("hello@mail.com");

        AssertUser.Equal(result!, user);
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var result = await repo.GetByUsernameOrEmailAsync("zzz");

        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // EMAIL EXISTS
    // ------------------------------------------------------------
    [Fact]
    public async Task EmailExistsAsync_ShouldReturnTrue_WhenExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        SeedHelper.AddUsers(db.Db, UserBuilder.Create(email: "exists@mail.com"));

        var exists = await repo.EmailExistsAsync("exists@mail.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var exists = await repo.EmailExistsAsync("missing@mail.com");

        exists.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // USERNAME EXISTS
    // ------------------------------------------------------------
    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnTrue_WhenExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        SeedHelper.AddUsers(db.Db, UserBuilder.Create(username: "foundUser"));

        var exists = await repo.UsernameExistsAsync("foundUser");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var exists = await repo.UsernameExistsAsync("missingUser");

        exists.Should().BeFalse();
    }
}
