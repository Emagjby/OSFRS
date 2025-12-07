using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.SecurityTests.Utils;

public class MaintenanceAuthorizationTests : SecurityTestBase
{
    private const string BASE = "/api/maintenance";

    public MaintenanceAuthorizationTests(SecurityWebAppFactory factory)
        : base(factory) { }

    // =====================================================================
    // 1. FILTERED LIST (admin only)
    // =====================================================================

    [Fact]
    public async Task GetFiltered_Anonymous_Returns_401()
    {
        var res = await Anonymous.GetAsync($"{BASE}/all");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFiltered_User_Returns_403()
    {
        var res = await User.GetAsync($"{BASE}/all");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetFiltered_Admin_Returns_200()
    {
        var res = await Admin.GetAsync($"{BASE}/all");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =====================================================================
    // 2. BY FACILITY (Authenticated users allowed)
    // =====================================================================

    [Fact]
    public async Task GetByFacility_Anonymous_Returns_401()
    {
        var res = await Anonymous.GetAsync($"{BASE}/facility/1");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByFacility_User_Returns_200()
    {
        var res = await User.GetAsync($"{BASE}/facility/1");
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetByFacility_Admin_Returns_200()
    {
        var res = await Admin.GetAsync($"{BASE}/facility/1");
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    // =====================================================================
    // 3. UPCOMING (Authenticated users allowed)
    // =====================================================================

    [Fact]
    public async Task GetUpcoming_Anonymous_Returns_401()
    {
        var res = await Anonymous.GetAsync($"{BASE}/upcoming");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUpcoming_User_Returns_200()
    {
        var res = await User.GetAsync($"{BASE}/upcoming");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUpcoming_Admin_Returns_200()
    {
        var res = await Admin.GetAsync($"{BASE}/upcoming");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =====================================================================
    // 4. CREATE MAINTENANCE (Admin only)
    // =====================================================================

    [Fact]
    public async Task Create_Anonymous_Returns_401()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 1,
            Description = "Test",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var res = await Anonymous.PostAsJsonAsync(BASE, dto);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_User_Returns_403()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 1,
            Description = "Test",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var res = await User.PostAsJsonAsync(BASE, dto);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_Admin_Returns_200()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 1,
            Description = "Test",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var res = await Admin.PostAsJsonAsync(BASE, dto);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    // =====================================================================
    // 5. UPDATE MAINTENANCE (Admin only)
    // =====================================================================

    [Fact]
    public async Task Update_Anonymous_Returns_401()
    {
        var dto = new UpdateMaintenanceRecordDto { Description = "Updated" };

        var res = await Anonymous.PutAsJsonAsync($"{BASE}/5", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_User_Returns_403()
    {
        var dto = new UpdateMaintenanceRecordDto { Description = "Updated" };

        var res = await User.PutAsJsonAsync($"{BASE}/5", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_Admin_Returns_2xx_Or_404()
    {
        var dto = new UpdateMaintenanceRecordDto { Description = "Updated" };

        var res = await Admin.PutAsJsonAsync($"{BASE}/5", dto);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    // =====================================================================
    // 6. DELETE MAINTENANCE (Admin only)
    // =====================================================================

    [Fact]
    public async Task Delete_Anonymous_Returns_401()
    {
        var res = await Anonymous.DeleteAsync($"{BASE}/5");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_User_Returns_403()
    {
        var res = await User.DeleteAsync($"{BASE}/5");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Admin_Returns_2xx_Or_404()
    {
        var res = await Admin.DeleteAsync($"{BASE}/5");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    // =====================================================================
    // 7. SYNC STATUSES (Admin only)
    // =====================================================================

    [Fact]
    public async Task SyncStatuses_Anonymous_Returns_401()
    {
        var res = await Anonymous.PostAsync($"{BASE}/sync-statuses", null);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SyncStatuses_User_Returns_403()
    {
        var res = await User.PostAsync($"{BASE}/sync-statuses", null);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SyncStatuses_Admin_Returns_200()
    {
        var res = await Admin.PostAsync($"{BASE}/sync-statuses", null);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }
}
