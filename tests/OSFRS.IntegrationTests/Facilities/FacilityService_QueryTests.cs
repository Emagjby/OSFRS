using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Facilities;

public class FacilityService_QueryTests : IntegrationTestBase
{
    public FacilityService_QueryTests()
        : base("OSFRS_IT_Facility_QueryTests") { }

    private IFacilityService Service() => Factory.FacilityService();

    private IFacilityRepository FacilityRepo() => Factory.FacilityRepo();

    private IMaintenanceRepository MaintenanceRepo() => Factory.MaintenanceRepo();

    private async Task<int> SeedFacility(string type = "Gym", string status = "Available")
    {
        var fac = FacilityBuilder.Create().WithType(type).WithStatus(status).Build();

        fac = await FacilityRepo().AddAsync(fac);
        await FacilityRepo().SaveChangesAsync();
        return fac!.Id;
    }

    // ================================================================
    // 1. GET ALL READONLY
    // ================================================================
    [Fact]
    public async Task GetAllReadonly_ShouldReturnAllFacilities()
    {
        await SeedFacility("Gym");
        await SeedFacility("Court");
        await SeedFacility("Pool");

        var result = await Service().GetAllReadonlyAsync();

        result.Should().HaveCount(3);
    }

    // ================================================================
    // 2. GET BY ID → FOUND
    // ================================================================
    [Fact]
    public async Task GetById_ShouldReturnMatchingFacility()
    {
        int id = await SeedFacility("Gym");

        var dto = await Service().GetByIdAsync(id);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(id);
        dto.Type.Should().Be("Gym");
    }

    // ================================================================
    // 3. GET BY ID → NOT FOUND RETURNS NULL
    // ================================================================
    [Fact]
    public async Task GetById_ShouldReturnNull_WhenNotFound()
    {
        var dto = await Service().GetByIdAsync(9999);

        dto.Should().BeNull();
    }

    // ================================================================
    // 4. DTO MAPPING VALIDATION
    // ================================================================
    [Fact]
    public async Task GetAllReadonly_ShouldReturnMappedDtos()
    {
        int id = await SeedFacility(type: "Pool", status: "Unavailable");

        var all = await Service().GetAllReadonlyAsync();

        var pool = all.Single(f => f.Id == id);

        pool.Type.Should().Be("Pool");
        pool.Status.Should().Be("Unavailable");
        pool.Name.Should().NotBeNullOrWhiteSpace();
    }

    // ================================================================
    // 5. TYPE GROUPING (MANUAL FILTERING)
    // ================================================================
    [Fact]
    public async Task GetAllReadonly_ShouldAllowClientSideTypeFiltering()
    {
        await SeedFacility("Gym");
        await SeedFacility("Gym");
        await SeedFacility("Court");

        var all = await Service().GetAllReadonlyAsync();

        var gyms = all.Where(f => f.Type == "Gym");

        gyms.Should().HaveCount(2);
    }

    // ================================================================
    // 6. IsFacilityAvailableAsync BASIC RULES
    // ================================================================
    [Theory]
    [InlineData("Available", true)]
    [InlineData("Unavailable", false)]
    [InlineData("UnderMaintenance", false)]
    public async Task IsFacilityAvailable_ShouldReflectStatus(string status, bool expected)
    {
        int id = await SeedFacility(status: status);

        var result = await Service().IsFacilityAvailableAsync(id);

        result.Should().Be(expected);
    }

    // ================================================================
    // 7. Under maintenance → availability false
    // ================================================================
    [Fact]
    public async Task IsFacilityAvailable_ShouldBeFalse_WhenUnderMaintenance()
    {
        int facId = await SeedFacility(status: "UnderMaintenance");

        var available = await Service().IsFacilityAvailableAsync(facId);

        available.Should().BeFalse();
    }

    // ================================================================
    // 8. Facilities under maintenance should not appear among "Available"
    // ================================================================
    [Fact]
    public async Task AvailableFacilities_ShouldExcludeUnderMaintenance()
    {
        int a = await SeedFacility("Gym", status: "Available");
        int b = await SeedFacility("Gym", status: "UnderMaintenance");
        int c = await SeedFacility("Court", status: "Available");

        var all = await Service().GetAllReadonlyAsync();
        var available = all.Where(f => f.Status == "Available");

        available.Should().HaveCount(2);
        available.Select(f => f.Id).Should().Contain(new[] { a, c });
        available.Select(f => f.Id).Should().NotContain(b);
    }
}
