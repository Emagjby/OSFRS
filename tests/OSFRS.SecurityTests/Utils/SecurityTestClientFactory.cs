using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.SecurityTests.Utils;

public class SecurityTestClientFactory
{
    private readonly SecurityWebAppFactory _factory;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepo;

    private bool _seeded = false;

    public SecurityTestClientFactory(SecurityWebAppFactory factory)
    {
        _factory = factory;

        var scope = _factory.Services.CreateScope();
        _tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        _userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        SeedUsersAsync().GetAwaiter().GetResult();
    }

    public IJwtTokenGenerator TokenGenerator => _tokenGenerator;

    // -------------------------------------------------------
    // USER SEEDING (User:1, Admin:999)
    // -------------------------------------------------------
    private async Task SeedUsersAsync()
    {
        if (_seeded)
            return;

        var all = await _userRepo.GetAllAsync();

        if (!all.Any(u => u.Id == 1))
        {
            await _userRepo.AddAsync(
                new User
                {
                    Id = 1,
                    Name = "TestUser",
                    Username = "testuser",
                    Email = "user@test.com",
                    Role = "User",
                    PasswordHash = "x",
                }
            );
        }

        if (!all.Any(u => u.Id == 2))
        {
            await _userRepo.AddAsync(
                new User
                {
                    Id = 2,
                    Name = "UserTwo",
                    Username = "usertwo",
                    Email = "usertwo@test.com",
                    Role = "User",
                    PasswordHash = "x",
                }
            );
        }

        if (!all.Any(u => u.Id == 999))
        {
            await _userRepo.AddAsync(
                new User
                {
                    Id = 999,
                    Name = "Admin",
                    Username = "admin",
                    Email = "admin@test.com",
                    Role = "Admin",
                    PasswordHash = "x",
                }
            );
        }

        await _userRepo.SaveChangesAsync();
        _seeded = true;
    }

    // -------------------------------------------------------
    // TOKEN GENERATION HELPERS
    // -------------------------------------------------------

    private string CreateUserToken(int id)
    {
        return _tokenGenerator.GenerateToken(
            new User
            {
                Id = id,
                Name = $"user{id}",
                Username = $"user{id}",
                Email = $"user{id}@test.com",
                Role = "User",
            }
        );
    }

    private string CreateAdminToken(int id)
    {
        return _tokenGenerator.GenerateToken(
            new User
            {
                Id = id,
                Name = $"admin{id}",
                Username = $"admin{id}",
                Email = $"admin{id}@test.com",
                Role = "Admin",
            }
        );
    }

    // -------------------------------------------------------
    // CLIENT FACTORY API
    // -------------------------------------------------------

    public HttpClient CreateAnonymousClient() => _factory.CreateClient();

    public HttpClient CreateUserClient(int userId = 1)
    {
        var token = CreateUserToken(userId);
        return CreateClientWithToken(token);
    }

    public HttpClient CreateAdminClient(int adminId = 999)
    {
        var token = CreateAdminToken(adminId);
        return CreateClientWithToken(token);
    }

    public HttpClient CreateClientWithToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
