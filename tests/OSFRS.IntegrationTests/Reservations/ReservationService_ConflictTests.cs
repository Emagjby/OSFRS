using FluentAssertions;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Reservations;

public class ReservationService_ConflictTests : IntegrationTestBase
{
    public ReservationService_ConflictTests()
        : base("OSFRS_IT_Reservation_ConflictTests") { }

    private IReservationService Service() => Factory.ReservationService();

    private IReservationRepository Repo() => Factory.ReservationRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private IMaintenanceRepository MaintenanceRepo() => Factory.MaintenanceRepo();

    // -------------------------------------------------------------
    // Facility helper WITH REAL ID
    // -------------------------------------------------------------
    private async Task<int> SeedFacility()
    {
        var fac = FacilityBuilder.Create().Build();
        fac = await FacilityRepo().AddAsync(fac);
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    // -------------------------------------------------------------
    // UPDATE → CONFLICT DETECTION
    // -------------------------------------------------------------

    [Fact]
    public async Task UpdateReservationAsync_ShouldFail_WhenNewTimeslotConflicts()
    {
        int facId = await SeedFacility();

        var existing1 = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(3))
            .Build();
        await Repo().AddAsync(existing1);

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(5)
            .WithStart(DateTime.UtcNow.AddHours(4))
            .WithEnd(DateTime.UtcNow.AddHours(5))
            .Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var fromRepo = await Repo().GetByIdAsync(target!.Id);

        var service = Service();

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
        };

        var act = async () => await service.UpdateReservationAsync(target.Id, dto, userId: 5);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*slot is already taken*");
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldSucceed_WhenMovingIntoFreeSlot()
    {
        int facId = await SeedFacility();

        var conflict = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(20)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(2))
            .Build();
        await Repo().AddAsync(conflict);

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(3))
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(6),
        };

        var result = await service.UpdateReservationAsync(target!.Id, dto, userId: 10);

        result.StartTime.Should().Be(dto.StartTime);
        result.EndTime.Should().Be(dto.EndTime);
    }

    // -------------------------------------------------------------
    // CANCELLED RESERVATIONS DO NOT BLOCK
    // -------------------------------------------------------------

    [Fact]
    public async Task UpdateReservationAsync_ShouldIgnoreCancelledReservations_WhenCheckingConflicts()
    {
        int facId = await SeedFacility();

        var cancelled = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(55)
            .WithStatus("Cancelled")
            .WithStart(DateTime.UtcNow.AddHours(2))
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();
        await Repo().AddAsync(cancelled);

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(5))
            .WithEnd(DateTime.UtcNow.AddHours(6))
            .Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        var updated = await service.UpdateReservationAsync(target!.Id, dto, userId: 10);

        updated.StartTime.Should().Be(dto.StartTime);
    }

    // -------------------------------------------------------------
    // MAINTENANCE BLOCKS UPDATE
    // -------------------------------------------------------------

    [Fact]
    public async Task UpdateReservationAsync_ShouldFail_WhenNewTimeOverlapsMaintenance()
    {
        int facId = await SeedFacility();

        var m = MaintenanceBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(2))
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();
        await MaintenanceRepo().AddAsync(m);
        await MaintenanceRepo().SaveChangesAsync();

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(5))
            .WithEnd(DateTime.UtcNow.AddHours(6))
            .Build();
        target = await Repo().AddAsync(target);
        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(5),
        };

        var act = async () => await service.UpdateReservationAsync(target!.Id, dto, userId: 10);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*maintenance*");
    }

    // -------------------------------------------------------------
    // OWNERSHIP + CANCELLED + PAST RULES
    // -------------------------------------------------------------

    [Fact]
    public async Task UpdateReservationAsync_ShouldFail_WhenUserTriesToEditOthersReservation()
    {
        int facId = await SeedFacility();

        var target = ReservationBuilder.Create().WithFacility(facId).WithUser(100).Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        var act = async () => await service.UpdateReservationAsync(target!.Id, dto, userId: 5);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*permission*");
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldFail_WhenReservationIsCancelledForNonAdmin()
    {
        int facId = await SeedFacility();

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStatus("Cancelled")
            .Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        var act = async () => await service.UpdateReservationAsync(target!.Id, dto, userId: 10);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*cancelled*");
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldFail_WhenReservationIsInPast_ForNonAdmin()
    {
        int facId = await SeedFacility();

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(10)
            .WithStart(DateTime.UtcNow.AddHours(-3))
            .WithEnd(DateTime.UtcNow.AddHours(-1))
            .Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var act = async () => await service.UpdateReservationAsync(target!.Id, dto, userId: 10);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*past*");
    }

    // -------------------------------------------------------------
    // ADMIN OVERRIDE
    // -------------------------------------------------------------

    [Fact]
    public async Task AdminUpdateReservationAsync_ShouldIgnoreConflicts()
    {
        int facId = await SeedFacility();

        var conflict = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(30)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();
        await Repo().AddAsync(conflict);

        var target = ReservationBuilder
            .Create()
            .WithFacility(facId)
            .WithUser(99)
            .WithStart(DateTime.UtcNow.AddHours(5))
            .WithEnd(DateTime.UtcNow.AddHours(6))
            .Build();
        target = await Repo().AddAsync(target);

        await Repo().SaveChangesAsync();

        var service = Service(); // AFTER seeding

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
            Status = "Confirmed",
        };

        var updated = await service.AdminUpdateReservationAsync(target!.Id, dto, adminId: 1);

        updated.StartTime.Should().Be(dto.StartTime);
        updated.Status.Should().Be("Confirmed");
    }
}
