using FluentAssertions;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Facilities;

public class FacilityService_AvailabilityTests : IntegrationTestBase
{
    public FacilityService_AvailabilityTests()
        : base("OSFRS_IT_Facility_AvailabilityTests") { }

    private IFacilityService Service() => Factory.FacilityService();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private IMaintenanceRepository MaintenanceRepo() => Factory.MaintenanceRepo();

    private async Task<int> SeedFacility(string status = "Available")
    {
        var fac = FacilityBuilder.Create().WithStatus(status).Build();
        fac = await FacilityRepo().AddAsync(fac);
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    private async Task AddMaintenance(int facilityId, string status)
    {
        var m = MaintenanceBuilder
            .Create()
            .WithFacility(facilityId)
            .WithStatus(status)
            .WithStart(DateTime.UtcNow.AddHours(-1))
            .WithEnd(DateTime.UtcNow.AddHours(1))
            .Build();

        await MaintenanceRepo().AddAsync(m);
        await MaintenanceRepo().SaveChangesAsync();
    }

    // ============================================================
    // A. SUCCESS CASES
    // ============================================================

    [Fact]
    public async Task UpdateAvailability_ShouldMarkAvailable_WhenNoMaintenance()
    {
        int facId = await SeedFacility("Unavailable");

        var result = await Service().UpdateAvailabilityAsync(facId, true);

        var updated = await FacilityRepo().GetByIdAsync(facId);

        result.Should().BeTrue();
        updated!.Status.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateAvailability_ShouldMarkAvailable_WhenMaintenanceNotInProgress()
    {
        int facId = await SeedFacility("Unavailable");

        await AddMaintenance(facId, status: "Scheduled"); // allowed

        var result = await Service().UpdateAvailabilityAsync(facId, true);

        var updated = await FacilityRepo().GetByIdAsync(facId);

        result.Should().BeTrue();
        updated!.Status.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateAvailability_ShouldMarkUnavailable_AlwaysAllowed()
    {
        int facId = await SeedFacility("Available");

        await AddMaintenance(facId, status: "InProgress"); // doesn’t matter for Unavailable

        var result = await Service().UpdateAvailabilityAsync(facId, false);

        var updated = await FacilityRepo().GetByIdAsync(facId);

        result.Should().BeFalse();
        updated!.Status.Should().Be("Unavailable");
    }

    [Fact]
    public async Task UpdateAvailability_ShouldRefreshUpdatedAt()
    {
        int facId = await SeedFacility("Unavailable");
        var before = (await FacilityRepo().GetByIdAsync(facId))!.UpdatedAt;

        await Task.Delay(10);

        await Service().UpdateAvailabilityAsync(facId, true);
        var updated = await FacilityRepo().GetByIdAsync(facId);

        updated!.UpdatedAt.Should().BeAfter(before);
    }

    // ============================================================
    // B. FAILURE CASES
    // ============================================================

    [Fact]
    public async Task UpdateAvailability_ShouldThrowNotFound_WhenFacilityDoesNotExist()
    {
        var act = async () => await Service().UpdateAvailabilityAsync(9999, true);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAvailability_ShouldFail_WhenMaintenanceInProgress()
    {
        int facId = await SeedFacility("Unavailable");

        await AddMaintenance(facId, status: "InProgress");

        var act = async () => await Service().UpdateAvailabilityAsync(facId, true);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*maintenance*");
    }

    [Fact]
    public async Task UpdateAvailability_ShouldNotChangeStatus_OnValidationFailure()
    {
        int facId = await SeedFacility("Unavailable");

        await AddMaintenance(facId, status: "InProgress"); // blocks marking Available

        var facilityBefore = await FacilityRepo().GetByIdAsync(facId);

        var act = async () => await Service().UpdateAvailabilityAsync(facId, true);
        await act.Should().ThrowAsync<ConflictException>();

        var facilityAfter = await FacilityRepo().GetByIdAsync(facId);

        facilityAfter!.Status.Should().Be(facilityBefore!.Status);
    }
}
