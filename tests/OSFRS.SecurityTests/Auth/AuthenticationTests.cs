using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.SecurityTests.Utils;

public class AuthenticationTests : SecurityTestBase
{
    public AuthenticationTests(SecurityWebAppFactory factory)
        : base(factory) { }

    // ----------------------------------------------------
    // 1. Anonymous can register
    // ----------------------------------------------------
    [Fact]
    public async Task Register_Works_For_Anonymous()
    {
        var payload = new UserRegistrationDto
        {
            Name = "John Test",
            Username = "john123",
            Email = "john@test.com",
            Password = "Password123!",
        };

        var res = await Anonymous.PostAsJsonAsync("/api/auth/register", payload);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ----------------------------------------------------
    // 2. Login returns a token
    // ----------------------------------------------------
    [Fact]
    public async Task Login_Returns_Jwt_Token()
    {
        // Register
        var reg = new UserRegistrationDto
        {
            Name = "Login User",
            Username = "loginuser",
            Email = "login@test.com",
            Password = "Password123!",
        };

        await Anonymous.PostAsJsonAsync("/api/auth/register", reg);

        // Login
        var login = new LoginRequestDto
        {
            UsernameOrEmail = "loginuser",
            Password = "Password123!",
        };

        var res = await Anonymous.PostAsJsonAsync("/api/auth/login", login);

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        body.Should().NotBeNull();
        body!.Should().ContainKey("token");
        body["token"].Should().NotBeNullOrEmpty();
    }

    // ----------------------------------------------------
    // 3. Token grants access to protected endpoint
    // ----------------------------------------------------
    [Fact]
    public async Task Login_Token_Allows_Access_To_Protected_Endpoint()
    {
        // Register
        var reg = new UserRegistrationDto
        {
            Name = "Protected Tester",
            Username = "protuser",
            Email = "prot@test.com",
            Password = "Pass123!",
        };

        await Anonymous.PostAsJsonAsync("/api/auth/register", reg);

        // Login
        var login = new LoginRequestDto { UsernameOrEmail = "protuser", Password = "Pass123!" };

        var loginRes = await Anonymous.PostAsJsonAsync("/api/auth/login", login);
        var body = await loginRes.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        var token = body!["token"];

        var authed = Clients.CreateClientWithToken(token);

        var res = await authed.GetAsync("/api/facility");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
