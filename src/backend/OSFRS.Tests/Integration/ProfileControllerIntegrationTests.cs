using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using OSFRS.Backend.Data;
using OSFRS.Backend.DTOs;

namespace OSFRS.Tests.Integration;

[Collection("IntegrationTests")]
public class ProfileControllerIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileControllerIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OSFRSDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    // Utility: registers and logs in, returns JWT token
    private async Task<string> RegisterAndLoginAsync()
    {
        var registerDto = new UserRegistrationDto
        {
            Name = "ProfileUser",
            Username = $"profile_{Guid.NewGuid():N}".Substring(0, 10),
            Email = $"profile_{Guid.NewGuid():N}@example.com",
            Password = "StrongPass123!"
        };

        // Register
        var registerResponse = await _client.PostAsJsonAsync("/api/user/register", registerDto);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Login
        var loginDto = new LoginRequestDto
        {
            UsernameOrEmail = registerDto.Username,
            Password = registerDto.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var raw = await loginResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(raw);
        Assert.NotNull(dict);
        Assert.True(dict!.ContainsKey("token") || dict.ContainsKey("jwt"));

        return dict.ContainsKey("token") ? dict["token"]! : dict["jwt"]!;
    }

    [Fact]
    public async Task GetProfile_WithValidJWT_ReturnsUserData()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/profile");
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            await PrintDebugResponse(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("profile_", body);
    }

    [Fact]
    public async Task GetProfile_WithoutJWT_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithValidJWT_UpdatesUserData()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updated = new UpdatedProfileDto
        {
            Name = "Updated Profile",
            Username = $"updated_{Guid.NewGuid():N}".Substring(0, 12),
            Email = $"updated_{Guid.NewGuid():N}@example.com",
            Password = "NewPass123!"
        };

        var response = await _client.PutAsJsonAsync("/api/profile", updated);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Profile updated successfully.", body);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidJWT_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var updated = new UpdatedProfileDto
        {
            Name = "Intruder",
            Username = "hacker_user",
            Email = "hack@example.com",
            Password = "WeakPass123!"
        };

        var response = await _client.PutAsJsonAsync("/api/profile", updated);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task PrintDebugResponse(HttpResponseMessage response)
    {
        Console.WriteLine("=== DEBUG RESPONSE START ===");
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine("Headers:");
        foreach (var h in response.Headers)
            Console.WriteLine($"  {h.Key}: {string.Join(", ", h.Value)}");

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Body:");
        Console.WriteLine(body);
        Console.WriteLine("=== DEBUG RESPONSE END ===");
    }
}