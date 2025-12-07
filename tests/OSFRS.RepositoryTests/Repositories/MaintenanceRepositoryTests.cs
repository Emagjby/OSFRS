using FluentAssertions;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class MaintenanceRepositoryTests
{
    private readonly Mock<IAppLogger<BaseRepository<MaintenanceRecord>>> _logger = new();

    private MaintenanceRepository CreateRepo(TestDbContext db) =>
        new MaintenanceRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // QUERYASYNC — FILTERS
    // ------------------------------------------------------------
    [Fact]
    public async Task QueryAsync_ShouldReturnAll_WhenNoFilters()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            MaintenanceBuilder.Create(status: "InProgress"),
            MaintenanceBuilder.Create(status: "Scheduled"),
            MaintenanceBuilder.Create(status: "Completed")
        );
        await db.Db.SaveChangesAsync();

        var result = await repo.QueryAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByStatus()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            MaintenanceBuilder.Create(status: "InProgress"),
            MaintenanceBuilder.Create(status: "Scheduled"),
            MaintenanceBuilder.Create(status: "InProgress")
        );
        await db.Db.SaveChangesAsync();

        var result = await repo.QueryAsync(status: "InProgress");

        result.Should().HaveCount(2);
        result.All(x => x.Status == "InProgress").Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByFacilityId()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            MaintenanceBuilder.Create(facilityId: 1),
            MaintenanceBuilder.Create(facilityId: 2),
            MaintenanceBuilder.Create(facilityId: 1)
        );
        await db.Db.SaveChangesAsync();

        var result = await repo.QueryAsync(facilityId: 1);

        result.Should().HaveCount(2);
        result.All(x => x.FacilityId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_ShouldApplyCombinedFilters()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            MaintenanceBuilder.Create(facilityId: 1, status: "Scheduled"),
            MaintenanceBuilder.Create(facilityId: 1, status: "InProgress"),
            MaintenanceBuilder.Create(facilityId: 2, status: "Scheduled")
        );
        await db.Db.SaveChangesAsync();

        var result = await repo.QueryAsync(status: "Scheduled", facilityId: 1);

        result.Should().HaveCount(1);
        result.First().FacilityId.Should().Be(1);
        result.First().Status.Should().Be("Scheduled");
    }

    // ------------------------------------------------------------
    // GETBYFACILITYASYNC
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByFacilityAsync_ShouldReturnOnlyThatFacility()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var r1 = MaintenanceBuilder.Create(facilityId: 1, start: DateUtils.UtcNowTrim);
        var r2 = MaintenanceBuilder.Create(facilityId: 2, start: DateUtils.UtcNowTrim);
        var r3 = MaintenanceBuilder.Create(facilityId: 1, start: DateUtils.UtcNowTrim.AddHours(1));

        await db.Db.AddRangeAsync(r1, r2, r3);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByFacilityAsync(1);

        result.Should().HaveCount(2);
        result.All(x => x.FacilityId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetByFacilityAsync_ShouldBeSortedByStartTimeDescending()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNowTrim;
        var r1 = MaintenanceBuilder.Create(facilityId: 3, start: t0);
        var r2 = MaintenanceBuilder.Create(facilityId: 3, start: t0.AddHours(3));
        var r3 = MaintenanceBuilder.Create(facilityId: 3, start: t0.AddHours(1));

        await db.Db.AddRangeAsync(r1, r2, r3);
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetByFacilityAsync(3)).ToList();

        result.Should().BeInDescendingOrder(x => x.StartTime);
    }

    // ------------------------------------------------------------
    // GETUPCOMINGASYNC
    // ------------------------------------------------------------
    [Fact]
    public async Task GetUpcomingAsync_ShouldReturnFutureOnly()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var now = DateUtils.UtcNowTrim;
        await db.Db.AddRangeAsync(
            MaintenanceBuilder.Create(start: now.AddHours(-2)),
            MaintenanceBuilder.Create(start: now.AddMinutes(1)),
            MaintenanceBuilder.Create(start: now.AddHours(3))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetUpcomingAsync()).ToList();

        result.Should().HaveCount(2);
        result.All(x => x.StartTime >= now).Should().BeTrue();
    }

    [Fact]
    public async Task GetUpcomingAsync_ShouldBeSortedDescending()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var now = DateUtils.UtcNowTrim;

        var r1 = MaintenanceBuilder.Create(start: now.AddHours(5));
        var r2 = MaintenanceBuilder.Create(start: now.AddHours(1));
        var r3 = MaintenanceBuilder.Create(start: now.AddHours(3));

        await db.Db.AddRangeAsync(r1, r2, r3);
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetUpcomingAsync()).ToList();

        result.Should().BeInDescendingOrder(x => x.StartTime);
    }
}
