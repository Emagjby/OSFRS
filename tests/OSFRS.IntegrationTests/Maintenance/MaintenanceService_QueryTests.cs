using FluentAssertions;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Maintenance;

public class MaintenanceService_QueryTests : IntegrationTestBase
{
    public MaintenanceService_QueryTests()
        : base("OSFRS_IT_Maintenance_QueryTests") { }

    private IMaintenanceService Service() => Factory.MaintenanceService();

    private IMaintenanceRepository Repo() => Factory.MaintenanceRepo();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private async Task<int> SeedFacility()
    {
        var fac = await FacilityRepo().AddAsync(FacilityBuilder.Create().Build());
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    // ============================================================
    // A. GetFilteredMaintenanceAsync
    // ============================================================
    [Fact]
    public async Task GetFiltered_ShouldReturnAll_WhenNoFilters()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder.Create().WithFacility(fac).Build();
        var m2 = MaintenanceBuilder.Create().WithFacility(fac).Build();
        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = await Service().GetFilteredMaintenanceAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFiltered_ShouldFilterByStatus()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder.Create().WithFacility(fac).WithStatus("Scheduled").Build();
        var m2 = MaintenanceBuilder.Create().WithFacility(fac).WithStatus("Completed").Build();
        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = await Service().GetFilteredMaintenanceAsync(status: "Scheduled");

        results.Should().ContainSingle().Which.Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task GetFiltered_ShouldFilterByFacility()
    {
        int f1 = await SeedFacility();
        int f2 = await SeedFacility();

        var m1 = MaintenanceBuilder.Create().WithFacility(f1).Build();
        var m2 = MaintenanceBuilder.Create().WithFacility(f2).Build();
        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = await Service().GetFilteredMaintenanceAsync(facilityId: f1);

        results.Should().ContainSingle().Which.FacilityId.Should().Be(f1);
    }

    [Fact]
    public async Task GetFiltered_ShouldFilterByStatusAndFacility()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder.Create().WithFacility(fac).WithStatus("Scheduled").Build();
        var m2 = MaintenanceBuilder.Create().WithFacility(fac).WithStatus("Completed").Build();
        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = await Service().GetFilteredMaintenanceAsync("Completed", fac);

        results.Should().ContainSingle().Which.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task GetFiltered_ShouldReturnEmpty_WhenNoMatches()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder.Create().WithFacility(fac).WithStatus("Scheduled").Build();
        await Repo().AddAsync(m1);
        await Repo().SaveChangesAsync();

        var results = await Service().GetFilteredMaintenanceAsync("Completed", fac);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFiltered_ShouldBeCaseSensitive()
    {
        int fac = await SeedFacility();

        var m = MaintenanceBuilder.Create().WithFacility(fac).WithStatus("Scheduled").Build();

        await Repo().AddAsync(m);
        await Repo().SaveChangesAsync();

        var results = await Service().GetFilteredMaintenanceAsync(status: "scheduled");

        results.Should().BeEmpty("status comparison is case-sensitive");
    }

    // ============================================================
    // B. GetMaintenanceByFacilityAsync
    // ============================================================
    [Fact]
    public async Task GetMaintenanceByFacility_ShouldReturnRecords()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder.Create().WithFacility(fac).Build();
        var m2 = MaintenanceBuilder.Create().WithFacility(fac).Build();

        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = await Service().GetMaintenanceByFacilityAsync(fac);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMaintenanceByFacility_ShouldThrow_WhenFacilityNotFound()
    {
        var act = async () => await Service().GetMaintenanceByFacilityAsync(9999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetMaintenanceByFacility_ShouldReturnEmpty_WhenNoneExist()
    {
        int fac = await SeedFacility();

        var results = await Service().GetMaintenanceByFacilityAsync(fac);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMaintenanceByFacility_ShouldReturnDescendingByStartTime()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .Build();

        var m2 = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(5))
            .Build();

        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = (await Service().GetMaintenanceByFacilityAsync(fac)).ToList();

        results.First().StartTime.Should().Be(m2.StartTime);
    }

    // ============================================================
    // C. GetUpcomingMaintenanceAsync
    // ============================================================
    [Fact]
    public async Task GetUpcoming_ShouldReturnOnlyFuture()
    {
        int fac = await SeedFacility();

        var past = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(-5))
            .WithEnd(DateTime.UtcNow.AddHours(-3))
            .Build();

        var future = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(2))
            .WithEnd(DateTime.UtcNow.AddHours(4))
            .Build();

        await Repo().AddRangeAsync([past, future]);
        await Repo().SaveChangesAsync();

        var results = await Service().GetUpcomingMaintenanceAsync();

        results.Should().ContainSingle().Which.Id.Should().Be(future.Id);
    }

    [Fact]
    public async Task GetUpcoming_ShouldReturnInDescendingOrder()
    {
        int fac = await SeedFacility();

        var m1 = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(3))
            .Build();

        var m2 = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(8))
            .Build();

        await Repo().AddRangeAsync([m1, m2]);
        await Repo().SaveChangesAsync();

        var results = (await Service().GetUpcomingMaintenanceAsync()).ToList();

        results.First().Id.Should().Be(m2.Id);
    }

    [Fact]
    public async Task GetUpcoming_ShouldReturnEmpty_WhenNoFutureMaintenance()
    {
        int fac = await SeedFacility();

        var past = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStart(DateTime.UtcNow.AddHours(-5))
            .Build();

        await Repo().AddAsync(past);
        await Repo().SaveChangesAsync();

        var results = await Service().GetUpcomingMaintenanceAsync();

        results.Should().BeEmpty();
    }

    // (optional) Depending on business rule:
    [Fact]
    public async Task GetUpcoming_ShouldIncludeCancelled_IfInFuture()
    {
        int fac = await SeedFacility();

        var cancelled = MaintenanceBuilder
            .Create()
            .WithFacility(fac)
            .WithStatus("Cancelled")
            .WithStart(DateTime.UtcNow.AddHours(6))
            .Build();

        await Repo().AddAsync(cancelled);
        await Repo().SaveChangesAsync();

        var results = await Service().GetUpcomingMaintenanceAsync();

        results.Should().ContainSingle().Which.Status.Should().Be("Cancelled");
    }
}
