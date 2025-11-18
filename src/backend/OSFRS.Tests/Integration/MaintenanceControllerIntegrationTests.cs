// using System.Net.Http.Headers;
// using System.Text;
// using System.Text.Json;
// using OSFRS.Backend.DTOs;
// using Microsoft.Extensions.DependencyInjection;
// using OSFRS.Backend.Data;
// using System.Net;
// using OSFRS.Models.Entities;

// namespace OSFRS.Tests.Integration;

// [Collection("IntegrationTests")]
// public class MaintenanceControllerIntegrationTests : IClassFixture<TestApplicationFactory>
// {
//     private readonly TestApplicationFactory _factory;
//     private readonly HttpClient _client;

//     public MaintenanceControllerIntegrationTests(TestApplicationFactory factory)
//     {
//         _factory = factory;
//         _client = factory.CreateClient();
//     }
//     private async Task<string> GetUserJwtAsync()
//     {
//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
//             var user = db.Users.FirstOrDefault(u => u.Username == "testuser_integration");
//             if (user != null && user.Role != "User")
//             {
//                 user.Role = "User";
//                 db.SaveChanges();
//             }
//         }

//         var loginDto = new LoginRequestDto
//         {
//             UsernameOrEmail = "testuser_integration",
//             Password = "StrongPass123!"
//         };

//         var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
//         var response = await _client.PostAsync("/api/auth/login", content);
//         response.EnsureSuccessStatusCode();

//         var responseString = await response.Content.ReadAsStringAsync();
//         using var doc = JsonDocument.Parse(responseString);
//         return doc.RootElement.GetProperty("token").GetString()!;
//     }

//     private async Task<string> GetAdminJwtAsync()
//     {
//         var loginDto = new LoginRequestDto
//         {
//             UsernameOrEmail = "testuser_integration",
//             Password = "StrongPass123!"
//         };

//         var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
//         var response = await _client.PostAsync("/api/auth/login", content);
//         response.EnsureSuccessStatusCode();

//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
//             var user = db.Users.FirstOrDefault(u => u.Username == "testuser_integration");
//             if (user != null && user.Role != "Admin")
//             {
//                 user.Role = "Admin";
//                 db.SaveChanges();
//             }
//         }

//         var adminLoginDto = new LoginRequestDto
//         {
//             UsernameOrEmail = "testuser_integration",
//             Password = "StrongPass123!"
//         };

//         var adminContent = new StringContent(JsonSerializer.Serialize(adminLoginDto), Encoding.UTF8, "application/json");
//         var adminResponse = await _client.PostAsync("/api/auth/login", adminContent);
//         adminResponse.EnsureSuccessStatusCode();

//         var adminResponseString = await adminResponse.Content.ReadAsStringAsync();
//         using var adminDoc = JsonDocument.Parse(adminResponseString);
//         return adminDoc.RootElement.GetProperty("token").GetString()!;
//     }

//     private void SetJwt(string token)
//     {
//         _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//     }

//     private void ClearJwt()
//     {
//         _client.DefaultRequestHeaders.Authorization = null;
//     }


//     [Fact]
//     public async Task ScheduleMaintenance_Admin_Valid_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "General Cleaning",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_Admin_DescriptionNull_ReturnsBadRequest()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = null!,
//             StartTime = DateTime.UtcNow.AddHours(3),
//             EndTime = DateTime.UtcNow.AddHours(4),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_Admin_MultipleCreations_AllOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         for (int i = 0; i < 3; i++)
//         {
//             var dto = new CreateMaintenanceRecordDto
//             {
//                 FacilityId = facilityId,
//                 Description = "Task " + i,
//                 StartTime = DateTime.UtcNow.AddHours(1 + i * 2),
//                 EndTime = DateTime.UtcNow.AddHours(2 + i * 2),
//                 Status = "Scheduled"
//             };

//             var content = TestUtils.JsonContent(dto);

//             var response = await _client.PostAsync("/api/maintenance", content);

//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//         }

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_Admin_ExactWindow_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var start = DateTime.UtcNow.AddHours(5);
//         var end = start.AddMinutes(1); 

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Very short maintenance",
//             StartTime = start,
//             EndTime = end,
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_Admin_FacilityAvailableOrUnavailable_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId, status: "Available");

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Test",
//             StartTime = DateTime.UtcNow.AddHours(2),
//             EndTime = DateTime.UtcNow.AddHours(3),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }


//     [Fact]
//     public async Task ScheduleMaintenance_EndBeforeStart_ReturnsBadRequest()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Invalid time window",
//             StartTime = DateTime.UtcNow.AddHours(4),
//             EndTime = DateTime.UtcNow.AddHours(3), // INVALID
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_MissingRequiredFields_ReturnsBadRequest()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         var facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Missing required fields",
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_InvalidJson_ReturnsBadRequest()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         var invalidJson = new StringContent(
//             "{ \"facilityId\": ,,, \"bad\": json }",
//             Encoding.UTF8,
//             "application/json"
//         );

//         var response = await _client.PostAsync("/api/maintenance", invalidJson);

//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//     }


//     [Fact]
//     public async Task ScheduleMaintenance_FacilityNotFound_ReturnsConflict()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = 9999,
//             Description = "Test maintenance",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
//     }


//     [Fact]
//     public async Task ScheduleMaintenance_UserForbidden_Returns403()
//     {
//         var userToken = await GetUserJwtAsync();
//         SetJwt(userToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Unauthorized attempt",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_NoJwt_Returns401()
//     {
//         ClearJwt();

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = 1,
//             Description = "Should fail",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_InvalidJwt_Returns401()
//     {
//         _client.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", "invalid.invalid.invalid");

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = 1,
//             Description = "Invalid JWT test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var content = TestUtils.JsonContent(dto);

//         var response = await _client.PostAsync("/api/maintenance", content);

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }



//     [Fact]
//     public async Task UpdateMaintenance_Admin_Existing_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Initial",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResponse = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResponse.EnsureSuccessStatusCode();

//         var createdJson = await TestUtils.ReadJson(createResponse);
//         int recordId = createdJson.GetProperty("id").GetInt32();

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             Description = "Updated",
//             StartTime = DateTime.UtcNow.AddHours(3),
//             EndTime = DateTime.UtcNow.AddHours(4),
//             Status = "Pending"
//         };

//         var updateResponse = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_Admin_PartialDescription_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Old description",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         var created = await TestUtils.ReadJson(createResp);
//         int recordId = created.GetProperty("id").GetInt32();

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             Description = "New desc"
//             // No StartTime, EndTime, Status provided
//         };

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_Admin_UpdateStartOnly_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Start test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         var created = await TestUtils.ReadJson(createResp);
//         int recordId = created.GetProperty("id").GetInt32();

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             StartTime = DateTime.UtcNow.AddHours(5)
//             // No other fields changed
//         };

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_Admin_UpdateEndOnly_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "End update test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         int recordId = (await TestUtils.ReadJson(createResp)).GetProperty("id").GetInt32();

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             EndTime = DateTime.UtcNow.AddHours(10)
//             // No other fields changed
//         };

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_Admin_NoChanges_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "No changes test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();

//         int recordId = (await TestUtils.ReadJson(createResp)).GetProperty("id").GetInt32();

//         var updateDto = new UpdateMaintenanceRecordDto();

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     } // to fix


//     [Fact]
//     public async Task UpdateMaintenance_EndBeforeStart_ReturnsBadRequest()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Valid record",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();

//         var created = await TestUtils.ReadJson(createResp);
//         int recordId = created.GetProperty("id").GetInt32();

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             StartTime = DateTime.UtcNow.AddHours(5),
//             EndTime = DateTime.UtcNow.AddHours(4) // INVALID
//         };

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.BadRequest, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_InvalidModel_ReturnsBadRequest()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Initial",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };
//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();

//         int recordId = (await TestUtils.ReadJson(createResp)).GetProperty("id").GetInt32();

//         var invalidJson = new StringContent(
//             "{ \"startTime\": \"not-a-date\", \"endTime\": 123 }",
//             Encoding.UTF8,
//             "application/json"
//         );

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             invalidJson
//         );

//         Assert.Equal(HttpStatusCode.BadRequest, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }


//     [Fact]
//     public async Task UpdateMaintenance_RecordNotFound_ReturnsNotFound()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int missingRecordId = 999999;

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             Description = "Should fail",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var response = await _client.PutAsync(
//             $"/api/maintenance/{missingRecordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_UserForbidden_Returns403()
//     {
//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         var created = await TestUtils.ReadJson(createResp);
//         int recordId = created.GetProperty("id").GetInt32();

//         var userToken = await GetUserJwtAsync();
//         SetJwt(userToken);

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             Description = "User should not update this"
//         };

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{recordId}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.Forbidden, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_NoJwt_Returns401()
//     {
//         ClearJwt();

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             Description = "Should fail",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2)
//         };

//         var response = await _client.PutAsync(
//             "/api/maintenance/1",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_InvalidJwt_Returns401()
//     {
//         _client.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", "invalid.invalid.invalid");

//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             Description = "Should fail",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2)
//         };

//         var response = await _client.PutAsync(
//             "/api/maintenance/1",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task DeleteMaintenance_Admin_Existing_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Delete test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         var createdJson = await TestUtils.ReadJson(createResp);
//         int recordId = createdJson.GetProperty("id").GetInt32();

//         var deleteResp = await _client.DeleteAsync($"/api/maintenance/{recordId}");

//         Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task DeleteMaintenance_Admin_AlreadyDeleted_ReturnsNotFound()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Double delete",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         int recordId = (await TestUtils.ReadJson(createResp)).GetProperty("id").GetInt32();

//         var deleteResp1 = await _client.DeleteAsync($"/api/maintenance/{recordId}");
//         Assert.Equal(HttpStatusCode.OK, deleteResp1.StatusCode);

//         var deleteResp2 = await _client.DeleteAsync($"/api/maintenance/{recordId}");
//         Assert.Equal(HttpStatusCode.NotFound, deleteResp2.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task DeleteMaintenance_Admin_NotFound_ReturnsNotFound()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int missingId = 999999;

//         var response = await _client.DeleteAsync($"/api/maintenance/{missingId}");

//         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//     }


//     [Fact]
//     public async Task DeleteMaintenance_UserForbidden_Returns403()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var createDto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "User forbidden test",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(createDto));
//         createResp.EnsureSuccessStatusCode();
//         int recordId = (await TestUtils.ReadJson(createResp)).GetProperty("id").GetInt32();

//         var userToken = await GetUserJwtAsync();
//         SetJwt(userToken);

//         var deleteResp = await _client.DeleteAsync($"/api/maintenance/{recordId}");

//         Assert.Equal(HttpStatusCode.Forbidden, deleteResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task DeleteMaintenance_NoJwt_Returns401()
//     {
//         ClearJwt();

//         var response = await _client.DeleteAsync("/api/maintenance/1");

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task DeleteMaintenance_InvalidJwt_Returns401()
//     {
//         _client.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", "invalid.invalid.invalid");

//         var response = await _client.DeleteAsync("/api/maintenance/1");

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task GetMaintenanceByFacility_ExistingFacility_WithRecords_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Record 1",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(2),
//             Status = "Scheduled"
//         };

//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto));
//         createResp.EnsureSuccessStatusCode();

//         var getResp = await _client.GetAsync($"/api/maintenance/facility/{facilityId}");

//         Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

//         var json = await TestUtils.ReadJson(getResp);
//         Assert.True(json.GetArrayLength() >= 1);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task GetMaintenanceByFacility_ExistingFacility_NoRecords_ReturnsEmpty()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var response = await _client.GetAsync($"/api/maintenance/facility/{facilityId}");

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task GetMaintenanceByFacility_FacilityNotFound_ReturnsNotFound()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int missingId = 999999;

//         var response = await _client.GetAsync($"/api/maintenance/facility/{missingId}");

//         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//     }


//     [Fact]
//     public async Task GetMaintenanceByFacility_NoJwt_Returns401()
//     {
//         ClearJwt();

//         var response = await _client.GetAsync("/api/maintenance/facility/1");

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task GetMaintenanceByFacility_InvalidJwt_Returns401()
//     {
//         _client.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", "invalid.invalid.invalid");

//         var response = await _client.GetAsync("/api/maintenance/facility/1");

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task GetUpcomingMaintenance_HasResults_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Upcoming",
//             StartTime = DateTime.UtcNow.AddHours(2),
//             EndTime = DateTime.UtcNow.AddHours(3),
//             Status = "Scheduled"
//         };

//         var createResp = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto));
//         createResp.EnsureSuccessStatusCode();

//         var response = await _client.GetAsync("/api/maintenance/upcoming");

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         var json = await TestUtils.ReadJson(response);
//         Assert.True(json.GetArrayLength() >= 1);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task GetUpcomingMaintenance_Empty_ReturnsEmpty()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         var response = await _client.GetAsync("/api/maintenance/upcoming");

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//     }


//     [Fact]
//     public async Task GetUpcomingMaintenance_NoJwt_Returns401()
//     {
//         ClearJwt();

//         var response = await _client.GetAsync("/api/maintenance/upcoming");

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task GetUpcomingMaintenance_InvalidJwt_Returns401()
//     {
//         _client.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", "invalid.invalid.invalid");

//         var response = await _client.GetAsync("/api/maintenance/upcoming");

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task SyncStatuses_Admin_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         var response = await _client.PostAsync("/api/maintenance/sync-statuses", null);

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//     }

//     [Fact]
//     public async Task SyncStatuses_Admin_UpdatesStatusesCorrectly_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId, status: "Available");

//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

//             db.MaintenanceRecords.Add(new MaintenanceRecord
//             {
//                 FacilityId = facilityId,
//                 Description = "Ongoing Maintenance",
//                 StartTime = DateTime.UtcNow.AddMinutes(-10),
//                 EndTime = DateTime.UtcNow.AddMinutes(10),
//                 Status = "InProgress"
//             });

//             await db.SaveChangesAsync();
//         }

//         var response = await _client.PostAsync("/api/maintenance/sync-statuses", null);
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         // VERIFY THE STATUS IS UPDATED
//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
//             var facility = db.Facilities.First(f => f.Id == facilityId);

//             Assert.Equal("UnderMaintenance", facility.Status);
//         }

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }


//     [Fact]
//     public async Task SyncStatuses_UserForbidden_Returns403()
//     {
//         var userToken = await GetUserJwtAsync();
//         SetJwt(userToken);

//         var response = await _client.PostAsync("/api/maintenance/sync-statuses", null);

//         Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
//     }

//     [Fact]
//     public async Task SyncStatuses_NoJwt_Returns401()
//     {
//         ClearJwt();

//         var response = await _client.PostAsync("/api/maintenance/sync-statuses", null);

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task SyncStatuses_InvalidJwt_Returns401()
//     {
//         _client.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", "invalid.invalid.invalid");

//         var response = await _client.PostAsync("/api/maintenance/sync-statuses", null);

//         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_OverlappingAllowed_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         // First record
//         var dto1 = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "First",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(3),
//             Status = "Scheduled"
//         };
//         var resp1 = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto1));
//         resp1.EnsureSuccessStatusCode();

//         // Overlapping record (allowed)
//         var dto2 = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Overlap",
//             StartTime = DateTime.UtcNow.AddHours(2), // overlaps with [1–3]
//             EndTime = DateTime.UtcNow.AddHours(4),
//             Status = "Scheduled"
//         };
//         var resp2 = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto2));

//         Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task UpdateMaintenance_IntoOverlap_Allowed_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = 1;
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         // Create existing maintenance record
//         var dto1 = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Existing",
//             StartTime = DateTime.UtcNow.AddHours(1),
//             EndTime = DateTime.UtcNow.AddHours(3),
//             Status = "Scheduled"
//         };
//         var resp1 = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto1));
//         resp1.EnsureSuccessStatusCode();
//         int rec1 = (await TestUtils.ReadJson(resp1)).GetProperty("id").GetInt32();

//         // Create second record
//         var dto2 = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Will update",
//             StartTime = DateTime.UtcNow.AddHours(4),
//             EndTime = DateTime.UtcNow.AddHours(5),
//             Status = "Scheduled"
//         };
//         var resp2 = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto2));
//         resp2.EnsureSuccessStatusCode();
//         int rec2 = (await TestUtils.ReadJson(resp2)).GetProperty("id").GetInt32();

//         // Update second to overlap first
//         var updateDto = new UpdateMaintenanceRecordDto
//         {
//             StartTime = DateTime.UtcNow.AddHours(2),
//             EndTime = DateTime.UtcNow.AddHours(4)
//         };

//         var updateResp = await _client.PutAsync(
//             $"/api/maintenance/{rec2}",
//             TestUtils.JsonContent(updateDto)
//         );

//         Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }


//     [Fact]
//     public async Task SyncStatuses_MaintenanceEnds_SetsAvailable()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = Random.Shared.Next(5000, 999999);
//         await TestUtils.CreateTestFacility(_factory, facilityId, status: "UnderMaintenance");

//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

//             db.MaintenanceRecords.Add(new MaintenanceRecord
//             {
//                 FacilityId = facilityId,
//                 Description = "Old",
//                 StartTime = DateTime.UtcNow.AddHours(-3),
//                 EndTime = DateTime.UtcNow.AddHours(-1),
//                 Status = "Completed"
//             });

//             await db.SaveChangesAsync();
//         }

//         var resp = await _client.PostAsync("/api/maintenance/sync-statuses", null);
//         Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
//             var facility = db.Facilities.First(f => f.Id == facilityId);
//             Assert.Equal("Available", facility.Status);
//         }

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task SyncStatuses_MaintenanceStarts_SetsUnderMaintenance()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = Random.Shared.Next(5000, 999999);
//         await TestUtils.CreateTestFacility(_factory, facilityId, status: "Available");

//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

//             db.MaintenanceRecords.Add(new MaintenanceRecord
//             {
//                 FacilityId = facilityId,
//                 Description = "Ongoing",
//                 StartTime = DateTime.UtcNow.AddMinutes(-30),
//                 EndTime = DateTime.UtcNow.AddMinutes(30),
//                 Status = "In Progress"
//             });

//             await db.SaveChangesAsync();
//         }

//         var resp = await _client.PostAsync("/api/maintenance/sync-statuses", null);
//         Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

//         using (var scope = _factory.Services.CreateScope())
//         {
//             var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
//             var facility = db.Facilities.First(f => f.Id == facilityId);

//             Assert.Equal("UnderMaintenance", facility.Status);
//         }

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

//     [Fact]
//     public async Task ScheduleMaintenance_PastMaintenance_ReturnsOk()
//     {
//         var adminToken = await GetAdminJwtAsync();
//         SetJwt(adminToken);

//         int facilityId = Random.Shared.Next(7000, 999999);
//         await TestUtils.CreateTestFacility(_factory, facilityId);

//         var dto = new CreateMaintenanceRecordDto
//         {
//             FacilityId = facilityId,
//             Description = "Past record",
//             StartTime = DateTime.UtcNow.AddHours(-5),
//             EndTime = DateTime.UtcNow.AddHours(-3),
//             Status = "Completed"
//         };

//         var response = await _client.PostAsync("/api/maintenance", TestUtils.JsonContent(dto));

//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//         await TestUtils.RemoveTestFacility(_factory, facilityId);
//     }

// }

// public static class TestUtils
// {
//     public static StringContent JsonContent(object obj)
//         => new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

//     public static async Task<JsonElement> ReadJson(HttpResponseMessage response)
//     {
//         var str = await response.Content.ReadAsStringAsync();
//         return JsonDocument.Parse(str).RootElement;
//     }

//     public static async Task CreateTestFacility(TestApplicationFactory factory, int id, string status = "Available")
//     {
//         using var scope = factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

//         db.Facilities.Add(new Facility
//         {
//             Id = id,
//             Name = $"Facility {id}",
//             Type = "Court",
//             Capacity = 10,
//             Status = status,
//             CreatedAt = DateTime.UtcNow,
//             UpdatedAt = DateTime.UtcNow
//         });

//         await db.SaveChangesAsync();
//     }

//     public static async Task UpdateFacilityStatus(TestApplicationFactory factory, int id, string status)
//     {
//         using var scope = factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

//         var facility = db.Facilities.First(f => f.Id == id);
//         facility.Status = status;
//         facility.UpdatedAt = DateTime.UtcNow;

//         await db.SaveChangesAsync();
//     }

//     public static async Task RemoveTestFacility(TestApplicationFactory factory, int id)
//     {
//         using var scope = factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();

//         var facility = await db.Facilities.FindAsync(id);
//         db.Facilities.Remove(facility!);

//         await db.SaveChangesAsync();
//     }
// }