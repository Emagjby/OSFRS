using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OSFRS.Backend.DTOs;

namespace OSFRS.Tests.Integration;

[Collection("Integration Tests")]
public class UsersControllerIntegrationTests : IntegrationTestBase, IClassFixture<TestApplicationFactory>
{
    public UsersControllerIntegrationTests(TestApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnOk_WhenUserIsCreated()
    {
        var dto = new UserRegistrationDto
        {
            Name = "John Doe",
            Username = "johndoe_integration",
            Email = "john_integration@example.com",
            Password = "StrongPass123!"
        };

        var response = await _client.PostAsJsonAsync("/api/user/register", dto);

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
            Username = "janedoe_integration",
            Email = "jane_integration@example.com",
            Password = "StrongPass123!"
        };

        var first = await _client.PostAsJsonAsync("/api/user/register", dto);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/user/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }
}