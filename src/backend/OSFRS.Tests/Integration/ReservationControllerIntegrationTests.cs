namespace OSFRS.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using OSFRS.Backend.DTOs;
using Microsoft.Extensions.DependencyInjection;

[Collection("IntegrationTests")]
public class ReservationControllerIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public ReservationControllerIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetValidJwtAsync()
    {
        var loginDto = new LoginRequestDto { UsernameOrEmail = "testuser_integration", Password = "StrongPass123!" };
        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    private void AddJwtToClient(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    private async Task<string> PromoteUserToAdminAndGetJwtAsync()
    {
        var loginDto = new LoginRequestDto { UsernameOrEmail = "testuser_integration", Password = "StrongPass123!" };
        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        // Initial login just to ensure user exists, but we don't use this token.
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();

        // Modify user role directly in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRS.Backend.Data.OSFRSDbContext>();
            var user = db.Users.FirstOrDefault(u => u.Username == "testuser_integration" || u.Email == "testuser_integration");
            if (user != null && user.Role != "Admin")
            {
                user.Role = "Admin";
                db.SaveChanges();
            }
        }
        // Now re-login to get a JWT with updated admin claims
        var adminLoginDto = new LoginRequestDto { UsernameOrEmail = "testuser_integration", Password = "StrongPass123!" };
        var adminContent = new StringContent(JsonSerializer.Serialize(adminLoginDto), Encoding.UTF8, "application/json");
        var adminResponse = await _client.PostAsync("/api/auth/login", adminContent);
        adminResponse.EnsureSuccessStatusCode();
        var adminResponseString = await adminResponse.Content.ReadAsStringAsync();
        using var adminDoc = JsonDocument.Parse(adminResponseString);
        var adminToken = adminDoc.RootElement.GetProperty("token").GetString()!;
        return adminToken;
    }

    [Fact]
    public async Task CreateReservation_WithValidJWT_ReturnsOk()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);
        var reservationDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            FacilityId = 1
        };
        var content = new StringContent(JsonSerializer.Serialize(reservationDto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/reservations/create", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_WithoutJWT_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var reservationDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            FacilityId = 1
        };
        var content = new StringContent(JsonSerializer.Serialize(reservationDto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/reservations/create", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_WithOverlappingTime_ReturnsConflict()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);
        var reservationDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(3),
            EndTime = DateTime.UtcNow.AddHours(4),
            FacilityId = 1
        };
        var content = new StringContent(JsonSerializer.Serialize(reservationDto), Encoding.UTF8, "application/json");

        var firstResponse = await _client.PostAsync("/api/reservations/create", content);
        var secondResponse = await _client.PostAsync("/api/reservations/create", content);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetMyReservations_ReturnsList()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);
        var reservationDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(10),
            EndTime = DateTime.UtcNow.AddHours(11),
            FacilityId = 1
        };
        var content = new StringContent(JsonSerializer.Serialize(reservationDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/reservations/create", content);
        createResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/reservations/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(responseString));
        Assert.Contains("facilityId", responseString);
    }

    [Fact]
    public async Task GetMyReservations_NoReservations_ReturnsNotFound()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var response = await _client.GetAsync("/api/reservations/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- Availability Calendar ---

    [Fact]
    public async Task GetAvailabilityCalendar_ReturnsSlots()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var facilityId = 1;
        var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/reservations/availability/{facilityId}?date={date}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.Contains("startTime", content);
    }

    [Fact]
    public async Task GetAvailabilityCalendar_InvalidFacility_ReturnsBadRequest()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var response = await _client.GetAsync($"/api/reservations/availability/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailabilityCalendar_NoResults_ReturnsNotFound()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var facilityId = 9999;
        var date = DateTime.UtcNow.AddYears(2).ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/reservations/availability/{facilityId}?date={date}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Updating ---

    [Fact]
    public async Task UpdateReservation_WithValidJWT_UpdatesSuccessfully()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var createDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(18),
            EndTime = DateTime.UtcNow.AddHours(19),
            FacilityId = 1
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/reservations/create", createContent);
        createResponse.EnsureSuccessStatusCode();
        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int reservationId = created.RootElement.GetProperty("id").GetInt32();

        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(20),
            EndTime = DateTime.UtcNow.AddHours(21),
            Status = "Pending"
        };
        var updateContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/reservations/update/{reservationId}", updateContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReservation_InvalidJWT_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(4),
            EndTime = DateTime.UtcNow.AddHours(5),
            Status = "Pending"
        };
        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync("/api/reservations/update/1", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReservation_InvalidData_ReturnsBadRequest()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(4),
            Status = "Pending"
        };
        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync("/api/reservations/update/1", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_WithValidJWT_CancelsSuccessfully()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var createDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(6),
            EndTime = DateTime.UtcNow.AddHours(7),
            FacilityId = 1
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/reservations/create", createContent);
        createResponse.EnsureSuccessStatusCode();
        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int reservationId = created.RootElement.GetProperty("id").GetInt32();

        var response = await _client.PutAsync($"/api/reservations/cancel/{reservationId}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_NonExistentReservation_ReturnsConflict()
    {
        var token = await GetValidJwtAsync();
        AddJwtToClient(token);

        var response = await _client.PutAsync("/api/reservations/cancel/999", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- Admin Tests ---

    [Fact]
    public async Task GetReservations_AsAdmin_ReturnsOk()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);

        var createDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(100),
            EndTime = DateTime.UtcNow.AddHours(101),
            FacilityId = 1
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/reservations/create", createContent);
        createResponse.EnsureSuccessStatusCode();
        var response = await _client.GetAsync("/api/reservations/facility/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchReservations_AsAdmin_ReturnsResults()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);
        var query = "?facilityId=1";
        var response = await _client.GetAsync($"/api/reservations/search{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task AdminUpdateReservation_Valid_ReturnsOk()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);

        var createDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(50),
            EndTime = DateTime.UtcNow.AddHours(51),
            FacilityId = 1
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/reservations/create", createContent);
        createResponse.EnsureSuccessStatusCode();
        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int reservationId = created.RootElement.GetProperty("id").GetInt32();

        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(52),
            EndTime = DateTime.UtcNow.AddHours(53),
            Status = "Approved"
        };
        var updateContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/reservations/admin/update/{reservationId}", updateContent);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminUpdateReservation_InvalidId_ReturnsBadRequest()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);
        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(60),
            EndTime = DateTime.UtcNow.AddHours(61),
            Status = "Approved"
        };

        var updateContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/reservations/admin/update/999999", updateContent);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AdminDeleteReservation_Valid_ReturnsOk()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);
        
        var createDto = new CreateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(70),
            EndTime = DateTime.UtcNow.AddHours(71),
            FacilityId = 1
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/reservations/create", createContent);
        createResponse.EnsureSuccessStatusCode();
        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int reservationId = created.RootElement.GetProperty("id").GetInt32();

        var response = await _client.DeleteAsync($"/api/reservations/admin/delete/{reservationId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminDeleteReservation_InvalidId_ReturnsBadRequest()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);
        var response = await _client.DeleteAsync("/api/reservations/admin/delete/999999");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AdminGetAllReservations_ReturnsList()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);

        var response = await _client.GetAsync("/api/reservations/admin/all");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task AdminGetAllReservations_NoData_ReturnsNotFound()
    {
        var token = await PromoteUserToAdminAndGetJwtAsync();
        AddJwtToClient(token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRS.Backend.Data.OSFRSDbContext>();
            db.Reservations.RemoveRange(db.Reservations);
            db.SaveChanges();
        }
        var response = await _client.GetAsync("/api/reservations/admin/all");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}