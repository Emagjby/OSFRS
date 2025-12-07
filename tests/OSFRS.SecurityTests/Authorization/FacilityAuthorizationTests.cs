using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.SecurityTests.Utils;

public class FacilityAuthorizationTests : SecurityTestBase
{
    private const string BASE = "/api/facility";

    public FacilityAuthorizationTests(SecurityWebAppFactory factory)
        : base(factory) { }

    // --------------------------------------------------------------------
    // 1. GET ALL — Anonymous -> 401
    // --------------------------------------------------------------------
    [Fact]
    public async Task GetAll_Anonymous_Returns_401()
    {
        var res = await Anonymous.GetAsync(BASE);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 2. GET ALL — User -> 200
    // --------------------------------------------------------------------
    [Fact]
    public async Task GetAll_User_Returns_200()
    {
        var res = await User.GetAsync(BASE);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --------------------------------------------------------------------
    // 3. GET ALL — Admin -> 200
    // --------------------------------------------------------------------
    [Fact]
    public async Task GetAll_Admin_Returns_200()
    {
        var res = await Admin.GetAsync(BASE);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --------------------------------------------------------------------
    // 4. GET BY ID — Anonymous -> 401
    // --------------------------------------------------------------------
    [Fact]
    public async Task GetById_Anonymous_Returns_401()
    {
        var res = await Anonymous.GetAsync($"{BASE}/1");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 5. GET BY ID — User -> 200 (allowed)
    // --------------------------------------------------------------------
    [Fact]
    public async Task GetById_User_Returns_200()
    {
        var res = await User.GetAsync($"{BASE}/1");
        // Service returns empty list, but user auth is OK.
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 6. CREATE — Anonymous -> 401
    // --------------------------------------------------------------------
    [Fact]
    public async Task Create_Anonymous_Returns_401()
    {
        var dto = new CreateFacilityDto
        {
            Name = "A",
            Type = "Gym",
            Capacity = 10,
        };

        var res = await Anonymous.PostAsJsonAsync(BASE, dto);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 7. CREATE — User -> 403 (Admin only)
    // --------------------------------------------------------------------
    [Fact]
    public async Task Create_User_Returns_403()
    {
        var dto = new CreateFacilityDto
        {
            Name = "A",
            Type = "Gym",
            Capacity = 10,
        };

        var res = await User.PostAsJsonAsync(BASE, dto);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --------------------------------------------------------------------
    // 8. CREATE — Admin -> 201
    // --------------------------------------------------------------------
    [Fact]
    public async Task Create_Admin_Returns_201()
    {
        var dto = new CreateFacilityDto
        {
            Name = "A",
            Type = "Gym",
            Capacity = 10,
        };

        var res = await Admin.PostAsJsonAsync(BASE, dto);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // --------------------------------------------------------------------
    // 9. UPDATE — Anonymous -> 401
    // --------------------------------------------------------------------
    [Fact]
    public async Task Update_Anonymous_Returns_401()
    {
        var dto = new UpdateFacilityDto { Name = "NewName" };

        var res = await Anonymous.PutAsJsonAsync($"{BASE}/1", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 10. UPDATE — User -> 403
    // --------------------------------------------------------------------
    [Fact]
    public async Task Update_User_Returns_403()
    {
        var dto = new UpdateFacilityDto { Name = "NewName" };

        var res = await User.PutAsJsonAsync($"{BASE}/1", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --------------------------------------------------------------------
    // 11. UPDATE — Admin -> 200
    // --------------------------------------------------------------------
    [Fact]
    public async Task Update_Admin_Returns_200()
    {
        var dto = new UpdateFacilityDto { Name = "NewName" };

        var res = await Admin.PutAsJsonAsync($"{BASE}/1", dto);
        // Facility does not exist, service will throw, but authorization passes.
        // We ONLY test authorization -> expect *not 403/401*.
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 12. DELETE — Anonymous -> 401
    // --------------------------------------------------------------------
    [Fact]
    public async Task Delete_Anonymous_Returns_401()
    {
        var res = await Anonymous.DeleteAsync($"{BASE}/1");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 13. DELETE — User -> 403
    // --------------------------------------------------------------------
    [Fact]
    public async Task Delete_User_Returns_403()
    {
        var res = await User.DeleteAsync($"{BASE}/1");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --------------------------------------------------------------------
    // 14. DELETE — Admin -> 200 or 404 (authorization OK regardless)
    // --------------------------------------------------------------------
    [Fact]
    public async Task Delete_Admin_Succeeds_Auth()
    {
        var res = await Admin.DeleteAsync($"{BASE}/1");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 15. GET AVAILABILITY — User -> 403
    // --------------------------------------------------------------------
    [Fact]
    public async Task Availability_User_Returns_403()
    {
        var res = await User.GetAsync($"{BASE}/1/availability");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --------------------------------------------------------------------
    // 16. GET AVAILABILITY — Admin -> 200
    // --------------------------------------------------------------------
    [Fact]
    public async Task Availability_Admin_Returns_200()
    {
        var res = await Admin.GetAsync($"{BASE}/1/availability");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------------------
    // 17. UPDATE AVAILABILITY — User -> 403
    // --------------------------------------------------------------------
    [Fact]
    public async Task UpdateAvailability_User_Returns_403()
    {
        var res = await User.PatchAsJsonAsync($"{BASE}/1/availability", true);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --------------------------------------------------------------------
    // 18. UPDATE AVAILABILITY — Admin -> 200
    // --------------------------------------------------------------------
    [Fact]
    public async Task UpdateAvailability_Admin_Returns_200()
    {
        var res = await Admin.PatchAsJsonAsync($"{BASE}/1/availability", true);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }
}
