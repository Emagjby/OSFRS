using FluentAssertions;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Reservations;

public class ReservationService_QueryTests : IntegrationTestBase
{
    public ReservationService_QueryTests()
        : base("OSFRS_IT_Reservation_QueryTests") { }

    private IReservationService Service() => Factory.ReservationService();

    private IReservationRepository Repo() => Factory.ReservationRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private async Task<int> SeedFacility()
    {
        var fac = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    // ---------------------------------------------------------------------
    // 1. AVAILABILITY CALENDAR TESTS
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAvailabilityCalendarAsync_ShouldReturnOnlyActiveReservations()
    {
        int facId = await SeedFacility();

        var a = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStatus("Pending")
            .WithStart(DateTime.UtcNow.Date.AddHours(8))
            .WithEnd(DateTime.UtcNow.Date.AddHours(10))
            .Build();

        var cancelled = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(20)
            .WithStatus("Cancelled")
            .WithStart(DateTime.UtcNow.Date.AddHours(12))
            .WithEnd(DateTime.UtcNow.Date.AddHours(14))
            .Build();

        await Repo().AddAsync(a);
        await Repo().AddAsync(cancelled);
        await Repo().SaveChangesAsync();

        var slots = (await Service().GetAvailabilityCalendarAsync(facId)).ToList();

        slots.Should().HaveCount(1);
        slots[0].Id.Should().Be(a.Id);
    }

    [Fact]
    public async Task GetAvailabilityCalendarAsync_ShouldThrowNotFound_WhenFacilityMissing()
    {
        var act = async () => await Service().GetAvailabilityCalendarAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ---------------------------------------------------------------------
    // 2. FACILITY RANGE QUERY TESTS
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetReservationsAsync_ShouldFilterByRangeCorrectly()
    {
        int facId = await SeedFacility();

        var inside = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(2))
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();

        var outside = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(10))
            .WithEnd(DateTime.UtcNow.AddHours(12))
            .Build();

        await Repo().AddAsync(inside);
        await Repo().AddAsync(outside);
        await Repo().SaveChangesAsync();

        var start = DateTime.UtcNow.AddHours(1);
        var end = DateTime.UtcNow.AddHours(5);

        var result = (await Service().GetReservationsAsync(facId, start, end)).ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(inside.Id);
    }

    [Fact]
    public async Task GetReservationsAsync_ShouldReturnAll_WhenNoRangeProvided()
    {
        int facId = await SeedFacility();

        var r1 = ReservationBuilder.Create().WithFacility(facId).Build();
        var r2 = ReservationBuilder.Create().WithFacility(facId).Build();

        await Repo().AddAsync(r1);
        await Repo().AddAsync(r2);
        await Repo().SaveChangesAsync();

        var result = (await Service().GetReservationsAsync(facId)).ToList();

        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------------------
    // 3. SEARCH TESTS
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchReservationAsync_ShouldFilterByUser()
    {
        int facId = await SeedFacility();

        var u1 = ReservationBuilder.Create().WithFacility(facId).WithUser(10).Build();
        var u2 = ReservationBuilder.Create().WithFacility(facId).WithUser(20).Build();

        await Repo().AddAsync(u1);
        await Repo().AddAsync(u2);
        await Repo().SaveChangesAsync();

        var result = (await Service().SearchReservationAsync(userId: 10)).ToList();

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(10);
    }

    [Fact]
    public async Task SearchReservationAsync_ShouldFilterByFacility()
    {
        int facA = await SeedFacility();
        int facB = await SeedFacility();

        var a = ReservationBuilder.Create().WithFacility(facA).Build();
        var b = ReservationBuilder.Create().WithFacility(facB).Build();

        await Repo().AddAsync(a);
        await Repo().AddAsync(b);
        await Repo().SaveChangesAsync();

        var result = (await Service().SearchReservationAsync(facilityId: facA)).ToList();

        result.Should().HaveCount(1);
        result[0].FacilityId.Should().Be(facA);
    }

    [Fact]
    public async Task SearchReservationAsync_ShouldFilterByDateRange()
    {
        int facId = await SeedFacility();

        var inside = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(2))
            .WithEnd(DateTime.UtcNow.AddHours(3))
            .Build();

        var before = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(-5))
            .WithEnd(DateTime.UtcNow.AddHours(-3))
            .Build();

        await Repo().AddAsync(inside);
        await Repo().AddAsync(before);
        await Repo().SaveChangesAsync();

        var from = DateTime.UtcNow.AddHours(1);
        var to = DateTime.UtcNow.AddHours(4);

        var result = (await Service().SearchReservationAsync(start: from, end: to)).ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(inside.Id);
    }

    [Fact]
    public async Task SearchReservationAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        int facId = await SeedFacility();

        var r = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .Build();

        await Repo().AddAsync(r);
        await Repo().SaveChangesAsync();

        var result = (
            await Service()
                .SearchReservationAsync(
                    start: DateTime.UtcNow.AddHours(10),
                    end: DateTime.UtcNow.AddHours(12)
                )
        ).ToList();

        result.Should().BeEmpty();
    }
}
