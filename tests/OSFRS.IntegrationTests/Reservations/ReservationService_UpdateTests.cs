using FluentAssertions;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Reservations;

public class ReservationService_UpdateTests : IntegrationTestBase
{
    public ReservationService_UpdateTests()
        : base("OSFRS_IT_Reservation_UpdateTests") { }

    private IReservationService Service() => Factory.ReservationService();

    private IReservationRepository Repo() => Factory.ReservationRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    // ------------------------------------------------------------
    // Helper: Seed facility and return its real ID
    // ------------------------------------------------------------
    private async Task<int> SeedFacility()
    {
        var fac = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    // ------------------------------------------------------------
    // 1. FULL UPDATE: START + END TIME
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateReservationAsync_ShouldUpdateTimes_WhenValid()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(2))
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(4),
        };

        var updated = await Service().UpdateReservationAsync(res!.Id, dto, userId: 10);

        updated.StartTime.Should().Be(dto.StartTime);
        updated.EndTime.Should().Be(dto.EndTime);
    }

    // ------------------------------------------------------------
    // 2. “ONLY START” (semantically): end stays same, but both provided
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateReservationAsync_ShouldUpdateOnlyStart_WhenEndNotProvided()
    {
        int facId = await SeedFacility();

        var originalEnd = DateTime.UtcNow.AddHours(4);

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(originalEnd)
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var newStart = DateTime.UtcNow.AddHours(2);

        var dto = new UpdateReservationDto
        {
            StartTime = newStart,
            EndTime = originalEnd, // required by validator, but logically "unchanged"
        };

        var updated = await Service().UpdateReservationAsync(res!.Id, dto, userId: 10);

        updated.StartTime.Should().Be(newStart);
        updated.EndTime.Should().Be(originalEnd);
    }

    // ------------------------------------------------------------
    // 3. “ONLY END” (semantically): start stays same, but both provided
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateReservationAsync_ShouldUpdateOnlyEnd_WhenStartNotProvided()
    {
        int facId = await SeedFacility();

        var originalStart = DateTime.UtcNow.AddHours(1);

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(originalStart)
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var newEnd = DateTime.UtcNow.AddHours(5);

        var dto = new UpdateReservationDto
        {
            StartTime = originalStart, // required
            EndTime = newEnd,
        };

        var updated = await Service().UpdateReservationAsync(res!.Id, dto, userId: 10);

        updated.StartTime.Should().Be(originalStart);
        updated.EndTime.Should().Be(newEnd);
    }

    // ------------------------------------------------------------
    // 4. ADMIN UPDATE STATUS
    // ------------------------------------------------------------
    [Fact]
    public async Task AdminUpdateReservationAsync_ShouldUpdateStatus_WhenValid()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStatus("Pending")
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var dto = new UpdateReservationDto
        {
            Status = "Confirmed",
            StartTime = res!.StartTime, // required by validator
            EndTime = res!.EndTime,
        };

        var updated = await Service().AdminUpdateReservationAsync(res!.Id, dto, adminId: 1);

        updated.Status.Should().Be("Confirmed");
    }

    // ------------------------------------------------------------
    // 5. UPDATEDAT SHOULD CHANGE ON TIME UPDATE
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateReservationAsync_ShouldRefreshUpdatedAt()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder.Create().WithFacility(facId).WithUser(10).Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var oldUpdatedAt = res!.UpdatedAt;

        await Task.Delay(10); // small delay to guarantee newer timestamp

        var dto = new UpdateReservationDto
        {
            StartTime = res.StartTime.AddHours(1),
            EndTime = res.EndTime.AddHours(1),
        };

        var updated = await Service().UpdateReservationAsync(res.Id, dto, userId: 10);

        updated.UpdatedAt.Should().BeAfter(oldUpdatedAt);
    }

    // ------------------------------------------------------------
    // 6. ADMIN CAN UPDATE SOMEONE ELSE’S RESERVATION
    // ------------------------------------------------------------
    [Fact]
    public async Task AdminUpdateReservationAsync_ShouldAllowUpdatingOthersReservation()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(500) // NOT the caller
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(2))
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(6),
            EndTime = DateTime.UtcNow.AddHours(7),
        };

        var updated = await Service().AdminUpdateReservationAsync(res!.Id, dto, adminId: 1);

        updated.StartTime.Should().Be(dto.StartTime);
        updated.EndTime.Should().Be(dto.EndTime);
    }

    // ------------------------------------------------------------
    // 7. ADMIN: UNSPECIFIED FIELDS STAY UNCHANGED
    // ------------------------------------------------------------
    [Fact]
    public async Task AdminUpdateReservationAsync_ShouldLeaveUnspecifiedFieldsIntact()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStatus("Pending")
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var originalStart = res!.StartTime;
        var originalEnd = res.EndTime;

        var dto = new UpdateReservationDto
        {
            Status = "Confirmed", // only logical change
            StartTime = originalStart, // required by validator
            EndTime = originalEnd,
        };

        var updated = await Service().AdminUpdateReservationAsync(res.Id, dto, adminId: 1);

        updated.StartTime.Should().Be(originalStart);
        updated.EndTime.Should().Be(originalEnd);
        updated.Status.Should().Be("Confirmed");
    }
}
