using FluentAssertions;
using Moq;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class FacilityRepositoryTests
{
    private readonly Mock<IAppLogger<BaseRepository<Facility>>> _logger = new();

    private FacilityRepository CreateRepo(TestDbContext db) =>
        new FacilityRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // IS FACILITY AVAILABLE
    // ------------------------------------------------------------
    [Fact]
    public async Task IsFacilityAvailableAsync_ShouldReturnTrue_WhenStatusIsAvailable()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var fac = FacilityBuilder.Create(status: "Available");
        await db.Db.Facilities.AddAsync(fac);
        await db.Db.SaveChangesAsync();

        var result = await repo.IsFacilityAvailableAsync(fac.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFacilityAvailableAsync_ShouldReturnFalse_WhenStatusIsUnavailable()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var fac = FacilityBuilder.Create(status: "Unavailable");
        await db.Db.Facilities.AddAsync(fac);
        await db.Db.SaveChangesAsync();

        var result = await repo.IsFacilityAvailableAsync(fac.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsFacilityAvailableAsync_ShouldThrow_WhenFacilityDoesNotExist()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var act = async () => await repo.IsFacilityAvailableAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ------------------------------------------------------------
    // UPDATE AVAILABILITY
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldSetStatusToAvailable()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var fac = FacilityBuilder.Create(status: "Unavailable");
        await db.Db.Facilities.AddAsync(fac);
        await db.Db.SaveChangesAsync();

        await repo.UpdateAvailabilityAsync(fac.Id, true);
        await db.Db.SaveChangesAsync();

        var result = await db.Db.Facilities.FindAsync(fac.Id);

        result!.Status.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldSetStatusToUnavailable()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var fac = FacilityBuilder.Create(status: "Available");
        await db.Db.Facilities.AddAsync(fac);
        await db.Db.SaveChangesAsync();

        await repo.UpdateAvailabilityAsync(fac.Id, false);
        await db.Db.SaveChangesAsync();

        var result = await db.Db.Facilities.FindAsync(fac.Id);

        result!.Status.Should().Be("Unavailable");
    }

    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldUpdateTimestamp()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var fac = FacilityBuilder.Create(status: "Available");
        await db.Db.Facilities.AddAsync(fac);
        await db.Db.SaveChangesAsync();

        var before = fac.UpdatedAt;

        await repo.UpdateAvailabilityAsync(fac.Id, false);
        await db.Db.SaveChangesAsync();

        var after = (await db.Db.Facilities.FindAsync(fac.Id))!.UpdatedAt;

        after.Should().BeAfter(before);
    }

    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldThrow_WhenFacilityDoesNotExist()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var act = async () => await repo.UpdateAvailabilityAsync(999, true);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
