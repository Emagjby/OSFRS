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

    private async Task RegisterTestUserAsync()
    {
        var userRegistration = new UserRegistrationDto
        {
            Name = "TestUser",
            Username = "testuser_integration",
            Email = "testuser_integration@example.com",
            Password = "StrongPass123!"
        };

        await _client.PostAsJsonAsync("/api/user/register", userRegistration);
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

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with {response.StatusCode}. Response body: {body}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("token", responseString.ToLower());
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