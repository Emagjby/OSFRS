using Microsoft.EntityFrameworkCore;
using Moq;
using OSFRS.Backend.Data;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Tests.Services;

public class AuthServiceTests
{
    private readonly OSFRSDbContext _context;
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly Mock<IAppLogger<AuthService>> _mockLogger;
    private readonly IAuthService _service;

    public AuthServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "SuperStrongSecretKeyForTesting_Only_ChangeMe_123456789");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "osfrs-test");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "osfrs-client");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", "30");

        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        var mockRepoLogger = new Mock<IAppLogger<UserRepository>>();
        _repo = new UserRepository(_context, mockRepoLogger.Object);
        _hasher = new PasswordHasher();
        _jwtGenerator = new JwtTokenGenerator();
        _mockLogger = new Mock<IAppLogger<AuthService>>();
        _service = new AuthService(_repo, _hasher, _jwtGenerator, _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var password = "Pass1234";
        var user = new User
        {
            Name = "John",
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = _hasher.Hash(password),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user);

        var dto = new LoginRequestDto
        {
            UsernameOrEmail = "johndoe",
            Password = password
        };

        string token = await _service.LoginAsync(dto);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Contains(".", token); 

        _mockLogger.Verify(l => l.LogInformation(It.Is<string>(msg => msg.Contains("Login attempt")), It.IsAny<object[]>()), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.Is<string>(msg => msg.Contains("Login successful")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsException()
    {
        var user = new User
        {
            Name = "Jane",
            Username = "janedoe",
            Email = "jane@example.com",
            PasswordHash = _hasher.Hash("Correct123"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user);

        var dto = new LoginRequestDto
        {
            UsernameOrEmail = "janedoe",
            Password = "Wrong123"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync(dto));
        Assert.Equal("Invalid credentials.", ex.Message);

        _mockLogger.Verify(l => l.LogWarning(It.Is<string>(msg => msg.Contains("Invalid login attempt")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserDoesNotExist_ThrowsException()
    {
        var dto = new LoginRequestDto
        {
            UsernameOrEmail = "nonexistent",
            Password = "Pass1234"
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync(dto));
        Assert.Equal("Invalid credentials.", ex.Message);

        _mockLogger.Verify(l => l.LogWarning(It.Is<string>(msg => msg.Contains("Invalid login attempt")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmailFormat_ThrowsArgumentException()
    {
        var dto = new LoginRequestDto
        {
            UsernameOrEmail = "not-an-email",
            Password = "short"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.LoginAsync(dto));

        _mockLogger.Verify(l => l.LogWarning(It.Is<string>(msg => msg.Contains("Invalid email format") || msg.Contains("Invalid login attempt")), It.IsAny<object[]>()), Times.Once);
    }
}