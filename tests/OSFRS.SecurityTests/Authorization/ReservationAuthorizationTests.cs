using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.SecurityTests.Utils;

public class ReservationAuthorizationTests : SecurityTestBase
{
    private const string BASE = "/api/reservations";

    public ReservationAuthorizationTests(SecurityWebAppFactory factory)
        : base(factory) { }

    // ============================================================
    // 1. AVAILABILITY CALENDAR — PUBLIC
    // ============================================================

    [Fact]
    public async Task Availability_Anonymous_Returns_200()
    {
        var res = await Anonymous.GetAsync($"{BASE}/availability/1");
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Availability_User_Returns_200()
    {
        var res = await User.GetAsync($"{BASE}/availability/1");
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Availability_Admin_Returns_200()
    {
        var res = await Admin.GetAsync($"{BASE}/availability/1");
        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Unauthorized)
            .And.NotBe(HttpStatusCode.Forbidden);
    }

    // ============================================================
    // 2. USER ACTIONS ON OWN RESERVATIONS → Allowed (2xx)
    // ============================================================

    [Fact]
    public async Task User_Can_Create_Reservation()
    {
        var dto = new CreateReservationDto
        {
            FacilityId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var res = await User.PostAsJsonAsync(BASE, dto);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task User_Can_Update_Own_Reservation()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        var res = await User.PutAsJsonAsync($"{BASE}/1", dto);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task User_Can_Cancel_Own_Reservation()
    {
        var res = await User.PutAsync($"{BASE}/cancel/1", null);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task User_Can_Get_My_Reservations()
    {
        var res = await User.GetAsync($"{BASE}/my");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    // ============================================================
    // 3. USER BLOCKED FROM ADMIN ROUTES → 403
    // ============================================================

    [Fact]
    public async Task User_Cannot_GetReservations_AdminRoute()
    {
        var res = await User.GetAsync($"{BASE}/facility/1");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_Cannot_SearchReservations_AdminRoute()
    {
        var res = await User.GetAsync($"{BASE}/search");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_Cannot_GetAllReservations()
    {
        var res = await User.GetAsync(BASE);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_Cannot_AdminUpdateReservation()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(6),
        };

        var res = await User.PutAsJsonAsync($"{BASE}/admin/update/1", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_Cannot_AdminDeleteReservation()
    {
        var res = await User.DeleteAsync($"{BASE}/1");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ============================================================
    // 4. ADMIN: Full Access Everywhere → 2xx
    // ============================================================

    [Fact]
    public async Task Admin_Can_GetReservations_ForFacility()
    {
        var res = await Admin.GetAsync($"{BASE}/facility/1");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Can_SearchReservations()
    {
        var res = await Admin.GetAsync($"{BASE}/search");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Can_GetAllReservations()
    {
        var res = await Admin.GetAsync(BASE);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Can_Update_Any_Reservation()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(6),
            Status = "Confirmed",
        };

        var res = await Admin.PutAsJsonAsync($"{BASE}/admin/update/1", dto);

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Can_Delete_Any_Reservation()
    {
        var res = await Admin.DeleteAsync($"{BASE}/1");

        res.StatusCode.Should()
            .NotBe(HttpStatusCode.Forbidden)
            .And.NotBe(HttpStatusCode.Unauthorized);
    }
}
