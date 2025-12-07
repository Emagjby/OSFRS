using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.SecurityTests.Utils;

public class ReservationOwnershipTests : SecurityTestBase
{
    private const string BASE = "/api/reservations";

    public ReservationOwnershipTests(SecurityWebAppFactory factory)
        : base(factory) { }

    private async Task<int> CreateFacilityAsync()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Court A",
            Type = "Gym",
            Capacity = 5,
        };

        var res = await Admin.PostAsJsonAsync("/api/facility", dto);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return int.Parse(body!["id"].ToString()!);
    }

    // -----------------------------------------------------------
    // 1. User A creates a reservation
    // -----------------------------------------------------------
    private async Task<int> CreateReservationAsync(
        HttpClient client,
        int facilityId,
        int offset = 0
    )
    {
        var dto = new CreateReservationDto
        {
            FacilityId = facilityId,
            StartTime = DateTime.UtcNow.AddHours(1 + offset),
            EndTime = DateTime.UtcNow.AddHours(2 + offset),
        };

        var res = await client.PostAsJsonAsync(BASE, dto);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var all = await Admin.GetAsync("/api/reservations");
        var body = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return int.Parse(body!["id"].ToString()!);
    }

    // -----------------------------------------------------------
    // 2. User B cannot update User A’s reservation -> 409
    // -----------------------------------------------------------
    [Fact]
    public async Task UserB_Cannot_Update_UserA_Reservation()
    {
        int facility = await CreateFacilityAsync();

        // User A
        int reservationId = await CreateReservationAsync(User, facility);

        // User B tries to update
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(4),
        };

        var res = await Clients.CreateUserClient(2).PutAsJsonAsync($"{BASE}/{reservationId}", dto);

        res.StatusCode.Should().Be(HttpStatusCode.Conflict); // ownership violation
    }

    // -----------------------------------------------------------
    // 3. User B cannot cancel User A’s reservation -> 409
    // -----------------------------------------------------------
    [Fact]
    public async Task UserB_Cannot_Cancel_UserA_Reservation()
    {
        int facility = await CreateFacilityAsync();

        int reservationId = await CreateReservationAsync(User, facility);

        var res = await Clients
            .CreateUserClient(2)
            .PutAsync($"{BASE}/cancel/{reservationId}", null);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------
    // 4. GET /my returns only user's reservations
    // -----------------------------------------------------------
    [Fact]
    public async Task GetMy_Returns_Only_Own_Reservations()
    {
        int facility = await CreateFacilityAsync();

        // A: 2 reservations
        await CreateReservationAsync(User, facility);
        await CreateReservationAsync(User, facility, offset: 6);

        // B: 1 reservation
        await CreateReservationAsync(Clients.CreateUserClient(2), facility, offset: 12);

        var userARes = await User.GetAsync($"{BASE}/my");

        userARes.StatusCode.Should().Be(HttpStatusCode.OK);

        var listA = await userARes.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        listA!.Count.Should().Be(2);

        var userBRes = await Clients.CreateUserClient(2).GetAsync($"{BASE}/my");
        var listB = await userBRes.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        listB!.Count.Should().Be(1);
    }

    // -----------------------------------------------------------
    // 5. Admin can update any reservation -> 200
    // -----------------------------------------------------------
    [Fact]
    public async Task Admin_Can_Update_Any_Reservation()
    {
        int facility = await CreateFacilityAsync();
        int reservationId = await CreateReservationAsync(User, facility);

        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3),
            Status = "Confirmed",
        };

        var res = await Admin.PutAsJsonAsync($"{BASE}/admin/update/{reservationId}", dto);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // -----------------------------------------------------------
    // 6. Admin can delete any reservation -> 200
    // -----------------------------------------------------------
    [Fact]
    public async Task Admin_Can_Delete_Any_Reservation()
    {
        int facility = await CreateFacilityAsync();
        int reservationId = await CreateReservationAsync(User, facility);

        var res = await Admin.DeleteAsync($"{BASE}/{reservationId}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
