using FluentAssertions;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Maintenance;

public class MaintenanceService_UpdateTests : IntegrationTestBase
{
    public MaintenanceService_UpdateTests()
        : base("OSFRS_IT_Maintenance_UpdateTests") { }

    private IMaintenanceService Service() => Factory.MaintenanceService();

    private IMaintenanceRepository Repo() => Factory.MaintenanceRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private async Task<int> SeedFacility()
    {
        var fac = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    private async Task<MaintenanceRecord> SeedRecord(int facilityId)
    {
        var r = MaintenanceBuilder
            .Create()
            .WithFacility(facilityId)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(3))
            .WithStatus("Scheduled")
            .Build();

        r = await Repo().AddAsync(r);
        await Repo().SaveChangesAsync();
        return r!;
    }

    // ============================================================
    // A. NOT FOUND + VALIDATION
    // ============================================================
    [Fact]
    public async Task Update_ShouldThrowNotFound_WhenRecordDoesNotExist()
    {
        var dto = new UpdateMaintenanceRecordDto { Description = "x" };

        var act = async () => await Service().UpdateMaintenanceAsync(9999, dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenInvalidData()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = DateTime.UtcNow.AddHours(-5), // invalid past start
        };

        var act = async () => await Service().UpdateMaintenanceAsync(rec.Id, dto);

        await act.Should().ThrowAsync<PastDateException>();
    }

    // ============================================================
    // B. PARTIAL UPDATES
    // ============================================================
    [Fact]
    public async Task Update_ShouldUpdateDescriptionOnly()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var dto = new UpdateMaintenanceRecordDto { Description = "Updated desc" };

        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.Description.Should().Be("Updated desc");
        updated.StartTime.Should().Be(rec.StartTime);
        updated.EndTime.Should().Be(rec.EndTime);
        updated.Status.Should().Be(rec.Status);
    }

    [Fact]
    public async Task Update_ShouldUpdateEndOnly()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var endtime = rec.EndTime.AddHours(2);

        var dto = new UpdateMaintenanceRecordDto { EndTime = endtime };

        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.EndTime.Should().Be(endtime);
    }

    [Fact]
    public async Task Update_ShouldUpdateStatusOnly()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var dto = new UpdateMaintenanceRecordDto { Status = "Completed" };

        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Update_ShouldUpdateMultipleFields()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var start = rec.StartTime.AddHours(1);
        var end = rec.EndTime.AddHours(1);

        var dto = new UpdateMaintenanceRecordDto
        {
            Description = "X",
            StartTime = start,
            EndTime = end,
            Status = "Completed",
        };

        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.Description.Should().Be("X");
        updated.Status.Should().Be("Completed");
        updated.StartTime.Should().Be(start);
        updated.EndTime.Should().Be(end);
    }

    // ============================================================
    // C. UNCHANGED FIELDS
    // ============================================================
    [Fact]
    public async Task Update_ShouldNotChangeUnspecifiedFields()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var originalStart = rec.StartTime;
        var originalEnd = rec.EndTime;
        var originalStatus = rec.Status;

        var dto = new UpdateMaintenanceRecordDto { Description = "Only desc changed" };

        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.Description.Should().Be("Only desc changed");
        updated.StartTime.Should().Be(originalStart);
        updated.EndTime.Should().Be(originalEnd);
        updated.Status.Should().Be(originalStatus);
    }

    // ============================================================
    // D. UpdatedAt
    // ============================================================
    [Fact]
    public async Task Update_ShouldRefreshUpdatedAt()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var oldUpdated = rec.UpdatedAt;
        await Task.Delay(10);

        var dto = new UpdateMaintenanceRecordDto { Description = "Updated" };
        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.UpdatedAt.Should().BeAfter(oldUpdated);
    }

    // ============================================================
    // E. Time validation edge cases
    // ============================================================
    [Fact]
    public async Task Update_ShouldFail_WhenEndBeforeStart()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = rec.StartTime.AddHours(2),
            EndTime = rec.StartTime.AddHours(1),
        };

        var act = async () => await Service().UpdateMaintenanceAsync(rec.Id, dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Update_ShouldFail_WhenStartInPast()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var dto = new UpdateMaintenanceRecordDto { StartTime = DateTime.UtcNow.AddHours(-10) };

        var act = async () => await Service().UpdateMaintenanceAsync(rec.Id, dto);

        await act.Should().ThrowAsync<PastDateException>();
    }

    // ============================================================
    // F. Status transitions
    // ============================================================
    [Fact]
    public async Task Update_ShouldUpdateStatus_WhenValid()
    {
        int fac = await SeedFacility();
        var rec = await SeedRecord(fac);

        var dto = new UpdateMaintenanceRecordDto { Status = "InProgress" };

        var updated = await Service().UpdateMaintenanceAsync(rec.Id, dto);

        updated!.Status.Should().Be("InProgress");
    }
}
