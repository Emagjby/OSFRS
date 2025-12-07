using FluentAssertions;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class ReservationRepositoryTests
{
    private readonly Mock<IAppLogger<BaseRepository<Reservation>>> _logger = new();

    private ReservationRepository CreateRepo(TestDbContext db) =>
        new ReservationRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // GET BY USER
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByUserAsync_ShouldReturnOnlyThisUsersReservations()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var r1 = ReservationBuilder.Create(userId: 1);
        var r2 = ReservationBuilder.Create(userId: 2);
        var r3 = ReservationBuilder.Create(userId: 1);

        await db.Db.AddRangeAsync(r1, r2, r3);
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetByUserAsync(1)).ToList();

        result.Should().HaveCount(2);
        result.All(r => r.UserId == 1).Should().BeTrue();
    }

    // ------------------------------------------------------------
    // GET ALL WITH USER
    // ------------------------------------------------------------
    [Fact]
    public async Task GetAllWithUserAsync_ShouldIncludeUserNavigation()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create();
        var r = ReservationBuilder.Create(userId: user.Id);

        await db.Db.AddAsync(user);
        await db.Db.AddAsync(r);
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetAllWithUserAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].User.Should().NotBeNull();
        result[0].User!.Id.Should().Be(user.Id);
    }

    // ------------------------------------------------------------
    // GET BY ID WITH USER
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_ShouldIncludeUser_WhenExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create();
        var r = ReservationBuilder.Create(userId: user.Id);

        await db.Db.AddAsync(user);
        await db.Db.AddAsync(r);
        await db.Db.SaveChangesAsync();

        var result = await repo.GetByIdAsync(r.Id);

        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        result.User!.Id.Should().Be(user.Id);
    }

    // ------------------------------------------------------------
    // SLOT AVAILABILITY / CONFLICTS
    // ------------------------------------------------------------
    [Fact]
    public async Task IsSlotAvailableAsync_ShouldReturnFalse_WhenOverlapExists()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNow;
        await db.Db.AddAsync(
            ReservationBuilder.Create(start: t0, end: t0.AddHours(2), facilityId: 1)
        );

        await db.Db.SaveChangesAsync();

        var available = await repo.IsSlotAvailableAsync(
            start: t0.AddMinutes(30),
            end: t0.AddHours(1),
            facilityId: 1
        );

        available.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlotAvailableAsync_ShouldIgnoreCancelledReservations()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNow;

        await db.Db.AddAsync(
            ReservationBuilder.Create(
                start: t0,
                end: t0.AddHours(1),
                facilityId: 1,
                status: "Cancelled"
            )
        );

        await db.Db.SaveChangesAsync();

        var available = await repo.IsSlotAvailableAsync(
            start: t0.AddMinutes(10),
            end: t0.AddMinutes(20),
            facilityId: 1
        );

        available.Should().BeTrue();
    }

    // ------------------------------------------------------------
    // HAS CONFLICT (EXCLUDE SELF)
    // ------------------------------------------------------------
    [Fact]
    public async Task HasConflictAsync_ShouldExcludeGivenId()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNow;

        var conflict = ReservationBuilder.Create(start: t0, end: t0.AddHours(1), facilityId: 1);

        await db.Db.AddAsync(conflict);
        await db.Db.SaveChangesAsync();

        var result = await repo.HasConflictAsync(
            facilityId: 1,
            start: t0.AddMinutes(10),
            end: t0.AddMinutes(20),
            excludeReservationId: conflict.Id
        );

        result.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // RANGE QUERIES
    // ------------------------------------------------------------
    [Fact]
    public async Task GetByFacilityAndRangeAsync_ShouldReturnOverlapping()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNow;
        var r1 = ReservationBuilder.Create(facilityId: 1, start: t0, end: t0.AddHours(2));
        var r2 = ReservationBuilder.Create(
            facilityId: 1,
            start: t0.AddHours(3),
            end: t0.AddHours(4)
        );
        var r3 = ReservationBuilder.Create(
            facilityId: 1,
            start: t0.AddHours(1),
            end: t0.AddHours(3)
        );

        await db.Db.AddRangeAsync(r1, r2, r3);
        await db.Db.SaveChangesAsync();

        var result = (
            await repo.GetByFacilityAndRangeAsync(
                facilityId: 1,
                start: t0.AddMinutes(30),
                end: t0.AddHours(1)
            )
        ).ToList();

        result.Should().Contain(r1);
    }

    // ------------------------------------------------------------
    // SEARCH
    // ------------------------------------------------------------
    [Fact]
    public async Task SearchAsync_ShouldApplyFiltersCorrectly()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var user = UserBuilder.Create();
        var t0 = DateUtils.UtcNow;

        var r1 = ReservationBuilder.Create(
            userId: user.Id,
            facilityId: 1,
            start: t0,
            end: t0.AddHours(1)
        );
        var r2 = ReservationBuilder.Create(
            userId: user.Id,
            facilityId: 2,
            start: t0.AddHours(1),
            end: t0.AddHours(2)
        );

        await db.Db.AddAsync(user);
        await db.Db.AddRangeAsync(r1, r2);
        await db.Db.SaveChangesAsync();

        var result = (await repo.SearchAsync(userId: user.Id, facilityId: 1)).ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(r1.Id);
    }

    // ------------------------------------------------------------
    // UPDATE STATUS
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var r = ReservationBuilder.Create(status: "Pending");
        await db.Db.AddAsync(r);
        await db.Db.SaveChangesAsync();

        var updated = await repo.UpdateStatusAsync(r.Id, "Completed");

        updated!.Status.Should().Be("Completed");
        updated.UpdatedAt.Should().BeCloseTo(DateUtils.UtcNow, TimeSpan.FromSeconds(2));
    }
}
