using FluentAssertions;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Reservations;

public class ReservationService_CreateTests : IntegrationTestBase
{
    public ReservationService_CreateTests()
        : base("OSFRS_IT_Reservation_CreateTests") { }

    private IReservationService CreateService() => Factory.ReservationService();

    private IReservationRepository Repo() => Factory.ReservationRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private IMaintenanceRepository MaintenanceRepo() => Factory.MaintenanceRepo();

    // -------------------------------------------------------------
    // HAPPY PATH
    // -------------------------------------------------------------
    [Fact]
    public async Task CreateReservationAsync_ShouldCreate_WhenValid()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility!.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var created = await service.CreateReservationAsync(dto, userId: 1);

        created.Id.Should().BeGreaterThan(0);
        created.FacilityId.Should().Be(dto.FacilityId);
        created.Status.Should().Be("Pending");
        created.StartTime.Should().Be(dto.StartTime);
        created.EndTime.Should().Be(dto.EndTime);
    }

    // -------------------------------------------------------------
    // CONFLICTS
    // -------------------------------------------------------------
    [Fact]
    public async Task CreateReservationAsync_ShouldFail_WhenConflictsWithExisting()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        await Repo()
            .AddAsync(
                ReservationBuilder
                    .Create()
                    .WithFacility(facility!.Id)
                    .WithStart(DateTime.UtcNow.AddHours(1))
                    .WithEnd(DateTime.UtcNow.AddHours(3))
                    .Build()
            );
        await Repo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility.Id,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
        };

        var act = async () => await service.CreateReservationAsync(dto, userId: 10);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*unavailable*");
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldAllow_WhenOtherIsCancelled()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        await Repo()
            .AddAsync(
                ReservationBuilder
                    .Create()
                    .WithFacility(facility!.Id)
                    .WithStart(DateTime.UtcNow.AddHours(1))
                    .WithEnd(DateTime.UtcNow.AddHours(3))
                    .WithStatus("Cancelled")
                    .Build()
            );
        await Repo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var created = await service.CreateReservationAsync(dto, userId: 10);
        created.Should().NotBeNull();
    }

    // -------------------------------------------------------------
    // MAINTENANCE BLOCKS
    // -------------------------------------------------------------
    [Fact]
    public async Task CreateReservationAsync_ShouldFail_WhenOverlappingMaintenance()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        await MaintenanceRepo()
            .AddAsync(
                MaintenanceBuilder
                    .Create()
                    .WithFacility(facility!.Id)
                    .WithStart(DateTime.UtcNow.AddHours(0.5))
                    .WithEnd(DateTime.UtcNow.AddHours(2))
                    .Build()
            );
        await MaintenanceRepo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        var act = async () => await service.CreateReservationAsync(dto, userId: 1);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*maintenance*");
    }

    // -------------------------------------------------------------
    // TIME VALIDATION
    // -------------------------------------------------------------
    [Fact]
    public async Task CreateReservationAsync_ShouldFail_WhenStartAfterEnd()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility!.Id,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(1),
        };

        var act = async () => await service.CreateReservationAsync(dto, userId: 5);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldFail_WhenStartInPast()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility!.Id,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(1),
        };

        var act = async () => await service.CreateReservationAsync(dto, userId: 5);

        await act.Should().ThrowAsync<PastDateException>().WithMessage("*past*");
    }

    // -------------------------------------------------------------
    // FACILITY VALIDATION
    // -------------------------------------------------------------
    [Fact]
    public async Task CreateReservationAsync_ShouldFail_WhenFacilityNotFound()
    {
        var service = CreateService();

        var dto = new CreateReservationDto
        {
            FacilityId = 999,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var act = async () => await service.CreateReservationAsync(dto, userId: 3);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldFail_WhenUserInvalid()
    {
        var service = CreateService();

        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();

        var dto = new CreateReservationDto
        {
            FacilityId = facility!.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var act = async () => await service.CreateReservationAsync(dto, userId: 0);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Invalid user*");
    }
}
