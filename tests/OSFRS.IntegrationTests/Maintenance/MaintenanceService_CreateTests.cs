using FluentAssertions;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Maintenance;

public class MaintenanceService_CreateTests : IntegrationTestBase
{
    public MaintenanceService_CreateTests()
        : base("OSFRS_IT_Maintenance_Create") { }

    private IMaintenanceService Service() => Factory.MaintenanceService();

    private IMaintenanceRepository Repo() => Factory.MaintenanceRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private async Task<int> SeedFacility()
    {
        var facility = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();
        return facility!.Id;
    }

    // ------------------------------------------------------------
    // 1. SUCCESS
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldCreateMaintenance_WhenValid()
    {
        int facId = await SeedFacility();

        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            Description = "AC Repair",
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
            Status = "Scheduled",
        };

        var created = await Service().ScheduleMaintenanceAsync(dto);

        created.Id.Should().BeGreaterThan(0);
        created.FacilityId.Should().Be(facId);
        created.Description.Should().Be("AC Repair");
        created.Status.Should().Be("Scheduled");
    }

    // ------------------------------------------------------------
    // 2. FACILITY DOES NOT EXIST
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldThrowNotFound_WhenFacilityDoesNotExist()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 9999,
            Description = "Ghost maintenance",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var act = async () => await Service().ScheduleMaintenanceAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Facility not found*");
    }

    // ------------------------------------------------------------
    // 3. START IN PAST
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldFail_WhenStartTimeIsInPast()
    {
        int facId = await SeedFacility();

        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            Description = "Invalid past maintenance",
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(1),
        };

        var act = async () => await Service().ScheduleMaintenanceAsync(dto);

        await act.Should().ThrowAsync<PastDateException>().WithMessage("*past*");
    }

    // ------------------------------------------------------------
    // 4. END BEFORE START
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldFail_WhenEndTimeBeforeStartTime()
    {
        int facId = await SeedFacility();

        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var act = async () => await Service().ScheduleMaintenanceAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ------------------------------------------------------------
    // 5. ZERO-LENGTH WINDOW
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldFail_WhenStartEqualsEnd()
    {
        int facId = await SeedFacility();
        var now = DateTime.UtcNow.AddHours(3);

        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = now,
            EndTime = now,
        };

        var act = async () => await Service().ScheduleMaintenanceAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ------------------------------------------------------------
    // 6. OVERLAP EXISTS → FAIL
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldFail_WhenOverlappingExistingMaintenance()
    {
        int facId = await SeedFacility();

        // existing: 10 → 12
        var existing = MaintenanceBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(10))
            .WithEnd(DateTime.UtcNow.AddHours(12))
            .Build();

        await Repo().AddAsync(existing);
        await Repo().SaveChangesAsync();

        // new: 11 → 13 (conflict)
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = DateTime.UtcNow.AddHours(11),
            EndTime = DateTime.UtcNow.AddHours(13),
        };

        var act = async () => await Service().ScheduleMaintenanceAsync(dto);

        await act.Should().ThrowAsync<ConflictException>();
    }

    // ------------------------------------------------------------
    // 7. ADJACENT BUT NOT OVERLAPPING → SUCCESS
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldCreate_WhenAdjacentButNotOverlapping()
    {
        int facId = await SeedFacility();

        // existing: 10 → 12
        var existing = MaintenanceBuilder
            .Create()
            .WithFacility(facId)
            .WithStart(DateTime.UtcNow.AddHours(10))
            .WithEnd(DateTime.UtcNow.AddHours(12))
            .Build();

        await Repo().AddAsync(existing);
        await Repo().SaveChangesAsync();

        // new: exactly after → 12 → 14
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = DateTime.UtcNow.AddHours(12),
            EndTime = DateTime.UtcNow.AddHours(14),
        };

        var created = await Service().ScheduleMaintenanceAsync(dto);

        created.StartTime.Should().Be(dto.StartTime);
        created.EndTime.Should().Be(dto.EndTime);
    }

    // ------------------------------------------------------------
    // 8. CUSTOM STATUS
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldUseProvidedStatus()
    {
        int facId = await SeedFacility();

        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(5),
            Status = "InProgress",
        };

        var created = await Service().ScheduleMaintenanceAsync(dto);

        created.Status.Should().Be("InProgress");
    }

    // ------------------------------------------------------------
    // 9. DEFAULT STATUS = SCHEDULED
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldDefaultToScheduled_WhenStatusNotProvided()
    {
        int facId = await SeedFacility();

        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(6),
        };

        var created = await Service().ScheduleMaintenanceAsync(dto);

        created.Status.Should().Be("Scheduled");
    }

    // ------------------------------------------------------------
    // 10. TIMESTAMPS
    // ------------------------------------------------------------
    [Fact]
    public async Task ShouldSetCreatedAndUpdatedAt()
    {
        int facId = await SeedFacility();
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = facId,
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(7),
        };

        var before = DateTime.UtcNow;

        var created = await Service().ScheduleMaintenanceAsync(dto);

        created.CreatedAt.Should().BeOnOrAfter(before);
        created.UpdatedAt.Should().BeOnOrAfter(before);
        created.CreatedAt.Should().Be(created.UpdatedAt);
    }
}
