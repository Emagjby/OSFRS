using System.Net;
using System.Text;
using System.Net.Http.Json;
using OSFRS.Backend.DTOs;

namespace OSFRS.Tests.Integration;

[Collection("IntegrationTests")]
public class AuthControllerIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // === Registration ===

    [Fact]
    public async Task ShouldReturnOk_WhenUserIsCreated()
    {
        var dto = new UserRegistrationDto
        {
            Name = "John Doe",
            Username = $"john_integration_{Guid.NewGuid():N}".Substring(0, 16),
            Email = $"john_{Guid.NewGuid():N}@example.com",
            Password = "StrongPass123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("User registered successfully", content);
    }

    [Fact]
    public async Task ShouldReturnBadRequest_WhenDuplicateUser()
    {
        var dto = new UserRegistrationDto
        {
            Name = "Jane Doe",
            Username = $"jane_integration_{Guid.NewGuid():N}".Substring(0, 16),
            Email = $"jane_{Guid.NewGuid():N}@example.com",
            Password = "StrongPass123!"
        };

        // Register first time
        var first = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        // Attempt to register again with same credentials
        var second = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    // === Login ===

    private async Task RegisterTestUserAsync()
    {
        var userRegistration = new UserRegistrationDto
        {
            Name = "TestUser",
            Username = "testuser_integration",
            Email = "testuser_integration@example.com",
            Password = "StrongPass123!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", userRegistration);
    }

    [Fact]
    public async Task ShouldReturnToken_WhenCredentialsAreValid()
    {
        await RegisterTestUserAsync();

        var loginData = new LoginRequestDto
        {
            UsernameOrEmail = "testuser_integration",
            Password = "StrongPass123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("token", body.ToLower());
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        await RegisterTestUserAsync();

        var loginData = new LoginRequestDto
        {
            UsernameOrEmail = "testuser_integration",
            Password = "WrongPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var loginData = new LoginRequestDto
        {
            UsernameOrEmail = "nonexistentuser",
            Password = "SomePassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
    }
}