using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Data;
using OSFRS.Models.Entities;
using System.Net;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Tests.Integration;

[Collection("IntegrationTests")]
public class FacilityControllerIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public FacilityControllerIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetUserJwtAsync()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
            var user = db.Users.FirstOrDefault(u => u.Username == "testuser_integration");
            if (user != null && user.Role != "User")
            {
                user.Role = "User";
                db.SaveChanges();
            }
        }

        var loginDto = new LoginRequestDto
        {
            UsernameOrEmail = "testuser_integration",
            Password = "StrongPass123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    private async Task<string> GetAdminJwtAsync()
    {
        var loginDto = new LoginRequestDto
        {
            UsernameOrEmail = "testuser_integration",
            Password = "StrongPass123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
            var user = db.Users.FirstOrDefault(u => u.Username == "testuser_integration");
            if (user != null && user.Role != "Admin")
            {
                user.Role = "Admin";
                db.SaveChanges();
            }
        }

        var adminLoginDto = new LoginRequestDto
        {
            UsernameOrEmail = "testuser_integration",
            Password = "StrongPass123!"
        };

        var adminContent = new StringContent(JsonSerializer.Serialize(adminLoginDto), Encoding.UTF8, "application/json");
        var adminResponse = await _client.PostAsync("/api/auth/login", adminContent);
        adminResponse.EnsureSuccessStatusCode();

        var adminResponseString = await adminResponse.Content.ReadAsStringAsync();
        using var adminDoc = JsonDocument.Parse(adminResponseString);
        return adminDoc.RootElement.GetProperty("token").GetString()!;
    }

    private void AddJwt(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private void ClearJwt()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }


    [Fact]
    public async Task GetAllFacilities_WithJwt_ReturnsOk_WhenDataExists()
    {
        var token = await GetUserJwtAsync();
        AddJwt(token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
            db.Facilities.Add(new Facility
            {
                Name = "Test Gym",
                Type = "Gym",
                Capacity = 50,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        var response = await _client.GetAsync("/api/facility");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllFacilities_WithJwt_ReturnsNotFound_WhenNoFacilities()
    {
        var token = await GetUserJwtAsync();
        AddJwt(token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
            db.Facilities.RemoveRange(db.Facilities);
            db.SaveChanges();
        }

        var response = await _client.GetAsync("/api/facility");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllFacilities_WithJwt_ResponseContainsExpectedFields()
    {
        var token = await GetUserJwtAsync();
        AddJwt(token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
            db.Facilities.Add(new Facility
            {
                Name = "Basketball Court",
                Type = "Court",
                Capacity = 10,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        var response = await _client.GetAsync("/api/facility");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("name", json.ToLower());
        Assert.Contains("type", json.ToLower());
        Assert.Contains("capacity", json.ToLower());
        Assert.Contains("status", json.ToLower());
    }

    [Fact]
    public async Task GetAllFacilities_WithoutJwt_ReturnsUnauthorized()
    {
        ClearJwt();

        var response = await _client.GetAsync("/api/facility");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFacilityById_WithJwt_ReturnsOk_WhenExists()
    {
        var token = await GetUserJwtAsync();
        AddJwt(token);

        int facilityId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

            var facility = new Facility
            {
                Name = "Swimming Pool",
                Type = "Pool",
                Capacity = 30,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Facilities.Add(facility);
            db.SaveChanges();

            facilityId = facility.Id;
        }

        var response = await _client.GetAsync($"/api/facility/{facilityId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Swimming Pool", content);
    }

    [Fact]
    public async Task GetFacilityById_WithJwt_ReturnsNotFound_WhenNotExists()
    {
        var token = await GetUserJwtAsync();
        AddJwt(token);

        var response = await _client.GetAsync("/api/facility/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFacilityById_WithoutJwt_ReturnsUnauthorized()
    {
        ClearJwt();

        var response = await _client.GetAsync("/api/facility/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFacility_AsAdmin_WithValidBody_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var dto = new CreateFacilityDto
        {
            Name = "Basketball Court",
            Type = "Court",
            Capacity = 20,
            Status = "Available"
        };

        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/facility", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateFacility_AsAdmin_CanCreateMultipleFacilities_AllOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        for (int i = 1; i <= 3; i++)
        {
            var dto = new CreateFacilityDto
            {
                Name = $"Facility {i}",
                Type = "Generic",
                Capacity = 10 * i,
                Status = "Available"
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/facility", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task CreateFacility_AsAdmin_WithInvalidBody_ReturnsBadRequest()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var dto = new CreateFacilityDto
        {
            Name = "",                 // invalid
            Type = "Court",
            Capacity = -10,            // invalid
            Status = "Available"
        };

        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/facility", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFacility_AsNormalUser_ReturnsForbidden()
    {
        var userToken = await GetUserJwtAsync();
        AddJwt(userToken);

        var dto = new CreateFacilityDto
        {
            Name = "Tennis Court",
            Type = "Court",
            Capacity = 5,
            Status = "Available"
        };

        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/facility", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateFacility_WithInvalidJwt_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this.is.not.valid.jwt");

        var dto = new CreateFacilityDto
        {
            Name = "Invalid Facility",
            Type = "Test",
            Capacity = 10,
            Status = "Available"
        };

        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/facility", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFacility_WithMalformedJson_ReturnsBadRequest()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var malformedJson = "{ \"name\": \"Bad JSON\", \"type\": ";  // intentionally broken

        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/facility", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFacility_AsAdmin_ExistingFacility_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Gym A",
            Type = "Gym",
            Capacity = 20,
            Status = "Available"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();


        var updateDto = new UpdateFacilityDto
        {
            Name = "Updated Gym A",
            Type = "Updated Gym",
            Capacity = 30,
            Status = "Unavailable"
        };

        var updateContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var updateResponse = await _client.PutAsync($"/api/facility/{facilityId}", updateContent);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateFacility_AsAdmin_PartialUpdate_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Tennis Court",
            Type = "Court",
            Capacity = 4,
            Status = "Available"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();

        
        var updateDto = new UpdateFacilityDto
        {
            Capacity = 10
        };

        var updateContent = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var updateResponse = await _client.PutAsync($"/api/facility/{facilityId}", updateContent);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedJson = await updateResponse.Content.ReadAsStringAsync();
        var updated = JsonDocument.Parse(updatedJson);

        Assert.Equal("Tennis Court", updated.RootElement.GetProperty("name").GetString());
        Assert.Equal("Court", updated.RootElement.GetProperty("type").GetString());
        Assert.Equal("Available", updated.RootElement.GetProperty("status").GetString());
        Assert.Equal(10, updated.RootElement.GetProperty("capacity").GetInt32());
    }

    [Fact]
    public async Task UpdateFacility_AsAdmin_NonExistentFacility_ReturnsNotFound()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var updateDto = new UpdateFacilityDto
        {
            Name = "DoesNotMatter",
            Type = "None"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync("/api/facility/999999", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFacility_AsNormalUser_ReturnsForbidden()
    {
        var userToken = await GetUserJwtAsync();
        AddJwt(userToken);

        var updateDto = new UpdateFacilityDto
        {
            Name = "New Name"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync("/api/facility/1", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFacility_AsAdmin_WithInvalidModel_ReturnsBadRequest()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var updateDto = new UpdateFacilityDto
        {
            Capacity = -50
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync("/api/facility/1", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFacility_WithInvalidJwt_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var updateDto = new UpdateFacilityDto
        {
            Name = "Test"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

        var response = await _client.PutAsync("/api/facility/1", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFacility_AsAdmin_ExistingFacility_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Delete Test Facility",
            Type = "Court",
            Capacity = 5,
            Status = "Available"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();

        var deleteResponse = await _client.DeleteAsync($"/api/facility/{facilityId}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteFacility_AsAdmin_NonExistentFacility_ReturnsNotFound()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var response = await _client.DeleteAsync("/api/facility/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFacility_AsNormalUser_ReturnsForbidden()
    {
        var userToken = await GetUserJwtAsync();
        AddJwt(userToken);

        var response = await _client.DeleteAsync("/api/facility/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFacility_WithoutJwt_ReturnsUnauthorized()
    {
        ClearJwt();

        var response = await _client.DeleteAsync("/api/facility/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_AsAdmin_ExistingFacility_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Availability Test Facility",
            Type = "Gym",
            Capacity = 20,
            Status = "Available"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();

        var response = await _client.GetAsync($"/api/facility/{facilityId}/availability");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("facilityId", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isAvailable", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAvailability_AsAdmin_NonExistentFacility_ReturnsNotFound()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var response = await _client.GetAsync("/api/facility/999999/availability");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_AsNormalUser_ReturnsForbidden()
    {
        var userToken = await GetUserJwtAsync();
        AddJwt(userToken);

        var response = await _client.GetAsync("/api/facility/1/availability");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_WithoutJwt_ReturnsUnauthorized()
    {
        ClearJwt();

        var response = await _client.GetAsync("/api/facility/1/availability");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_AsAdmin_SetAvailableTrue_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Facility A",
            Type = "Tennis Court",
            Capacity = 10,
            Status = "Unavailable"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();

        var patchContent = new StringContent("true", Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync($"/api/facility/{facilityId}/availability", patchContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_AsAdmin_SetAvailableFalse_ReturnsOk()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Facility B",
            Type = "Basketball Court",
            Capacity = 15,
            Status = "Available"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();

        var patchContent = new StringContent("false", Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync($"/api/facility/{facilityId}/availability", patchContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_AsAdmin_NonExistentFacility_ReturnsNotFound()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var patchContent = new StringContent("true", Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync("/api/facility/999999/availability", patchContent);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_AsNormalUser_ReturnsForbidden()
    {
        var userToken = await GetUserJwtAsync();
        AddJwt(userToken);

        var patchContent = new StringContent("true", Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync("/api/facility/1/availability", patchContent);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_WithoutJwt_ReturnsUnauthorized()
    {
        ClearJwt();

        var patchContent = new StringContent("true", Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync("/api/facility/1/availability", patchContent);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_AsAdmin_WithInvalidBody_ReturnsBadRequest()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Facility C",
            Type = "Pool",
            Capacity = 30,
            Status = "Available"
        };

        var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createdJson = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(createdJson);
        int facilityId = created.RootElement.GetProperty("id").GetInt32();

        var patchContent = new StringContent("\"NOT_A_BOOL\"", Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync($"/api/facility/{facilityId}/availability", patchContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // [Fact]
    // public async Task UpdateAvailability_AsAdmin_WhenUnderMaintenance_StillReturnsOk()
    // {
    //     var adminToken = await GetAdminJwtAsync();
    //     AddJwt(adminToken);

    //     var createDto = new CreateFacilityDto
    //     {
    //         Name = "Gym",
    //         Type = "Fitness",
    //         Capacity = 20,
    //         Status = "Available"
    //     };

    //     var createContent = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
    //     var createResponse = await _client.PostAsync("/api/facility", createContent);
    //     createResponse.EnsureSuccessStatusCode();

    //     var createdJson = await createResponse.Content.ReadAsStringAsync();
    //     var created = JsonDocument.Parse(createdJson);
    //     int facilityId = created.RootElement.GetProperty("id").GetInt32();

    //     var maintenanceDto = new CreateMaintenanceRecordDto
    //     {
    //         FacilityId = facilityId,
    //         Description = "Test Maintenance",
    //         StartTime = DateTime.UtcNow.AddMinutes(-30),
    //         EndTime = DateTime.UtcNow.AddMinutes(30),
    //         Status = "Scheduled"
    //     };

    //     var mContent = new StringContent(JsonSerializer.Serialize(maintenanceDto), Encoding.UTF8, "application/json");
    //     var mResponse = await _client.PostAsync("/api/maintenance", mContent);
    //     mResponse.EnsureSuccessStatusCode();

    //     using (var scope = _factory.Services.CreateScope())
    //     {
    //         var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();
    //         await maintenanceService.SyncFacilityStatusesAsync();
    //     }

    //     var patchContent = new StringContent("true", Encoding.UTF8, "application/json");
    //     var response = await _client.PatchAsync($"/api/facility/{facilityId}/availability", patchContent);

    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    // } // - to fix

    [Fact]
    public async Task CreateFacility_ThenCheckAvailability_ReturnsConsistentState()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Tennis Court Alpha",
            Type = "Tennis",
            Capacity = 4,
            Status = "Available"
        };

        var createBody = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", createBody);
        createResponse.EnsureSuccessStatusCode();

        var json = await createResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        int facilityId = doc.RootElement.GetProperty("id").GetInt32();

        var response = await _client.GetAsync($"/api/facility/{facilityId}/availability");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(content);

        bool isAvailable = parsed.RootElement.GetProperty("isAvailable").GetBoolean();

        Assert.True(isAvailable);
    }

    [Fact]
    public async Task DeleteFacility_ThenCheckAvailability_ReturnsNotFound()
    {
        var adminToken = await GetAdminJwtAsync();
        AddJwt(adminToken);

        var createDto = new CreateFacilityDto
        {
            Name = "Pool Omega",
            Type = "Swimming",
            Capacity = 12,
            Status = "Available"
        };

        var body = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/facility", body);
        createResponse.EnsureSuccessStatusCode();

        var json = await createResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        int facilityId = doc.RootElement.GetProperty("id").GetInt32();

        var deleteResponse = await _client.DeleteAsync($"/api/facility/{facilityId}");
        deleteResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync($"/api/facility/{facilityId}/availability");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}