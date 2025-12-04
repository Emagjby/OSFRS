using FluentAssertions;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Reservations;

public class ReservationService_CancelTests : IntegrationTestBase
{
    public ReservationService_CancelTests()
        : base("OSFRS_IT_Reservation_CancelTests") { }

    private IReservationService Service() => Factory.ReservationService();

    private IReservationRepository Repo() => Factory.ReservationRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    // facility helper
    private async Task<int> SeedFacility()
    {
        var fac = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    // ---------------------------------------------------------
    // 1. SUCCESSFUL CANCEL (OWNER)
    // ---------------------------------------------------------
    [Fact]
    public async Task CancelReservationAsync_ShouldCancel_WhenValid()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder.Create().WithFacility(facId).WithUser(10).Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        await Service().CancelReservationAsync(res!.Id, userId: 10);
        var updated = (await Service().GetReservationsAsync(facId)).FirstOrDefault(r =>
            r.Id == res!.Id
        );

        updated!.Status.Should().Be("Cancelled");
    }

    // ---------------------------------------------------------
    // 2. NOT FOUND
    // ---------------------------------------------------------
    [Fact]
    public async Task CancelReservationAsync_ShouldThrowNotFound_WhenIdDoesNotExist()
    {
        var act = async () => await Service().CancelReservationAsync(9999, userId: 10);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ---------------------------------------------------------
    // 3. USER NOT OWNER
    // ---------------------------------------------------------
    [Fact]
    public async Task CancelReservationAsync_ShouldFail_WhenUserIsNotOwner()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(100) // owner
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var act = async () => await Service().CancelReservationAsync(res!.Id, userId: 5);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*do not own*");
    }

    // ---------------------------------------------------------
    // 4. ALREADY CANCELLED
    // ---------------------------------------------------------
    [Fact]
    public async Task CancelReservationAsync_ShouldFail_WhenAlreadyCancelled()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStatus("Cancelled")
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var act = async () => await Service().CancelReservationAsync(res!.Id, userId: 10);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*already*");
    }

    // ---------------------------------------------------------
    // 5. PAST RESERVATION CANNOT BE CANCELLED
    // ---------------------------------------------------------
    [Fact]
    public async Task CancelReservationAsync_ShouldFail_WhenReservationIsInPast()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(-3))
            .WithEnd(DateTime.UtcNow.AddHours(-1))
            .Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var act = async () => await Service().CancelReservationAsync(res!.Id, userId: 10);

        await act.Should().ThrowAsync<PastDateException>().WithMessage("*past*");
    }

    // ---------------------------------------------------------
    // 6. UPDATEDAT SHOULD REFRESH
    // ---------------------------------------------------------
    [Fact]
    public async Task CancelReservationAsync_ShouldRefreshUpdatedAt()
    {
        int facId = await SeedFacility();

        var res = ReservationBuilder.Create().WithFacility(facId).WithUser(10).Build();

        res = await Repo().AddAsync(res);
        await Repo().SaveChangesAsync();

        var oldUpdatedAt = res!.UpdatedAt;

        await Task.Delay(10);

        await Service().CancelReservationAsync(res.Id, userId: 10);
        var updated = (await Service().GetReservationsAsync(facId)).FirstOrDefault(r =>
            r.Id == res!.Id
        );

        updated!.UpdatedAt.Should().BeAfter(oldUpdatedAt);
    }
}
