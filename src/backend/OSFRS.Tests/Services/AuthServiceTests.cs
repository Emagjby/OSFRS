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

public class AuthServiceTests : IDisposable
{
    private readonly OSFRSDbContext _context;
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly Mock<IAppLogger<AuthService>> _mockLogger;
    private readonly IAuthService _service;

    private readonly string? _oldSecret;
    private readonly string? _oldIssuer;
    private readonly string? _oldAudience;
    private readonly string? _oldExpiry;


    public AuthServiceTests()
    {
        //backup old vars
        _oldSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        _oldIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        _oldAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        _oldExpiry = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES");

        //test only vars
        Environment.SetEnvironmentVariable("JWT_SECRET", "Xo8pCrcllE87HPhyaBbR6bo2gN0gh/obKNGBhVb1r1U=");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
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

    public void Dispose()
    {
        if (_oldSecret is not null)
            Environment.SetEnvironmentVariable("JWT_SECRET", _oldSecret);

        if (_oldIssuer is not null)
            Environment.SetEnvironmentVariable("JWT_ISSUER", _oldIssuer);

        if (_oldAudience is not null)
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", _oldAudience);

        if (_oldExpiry is not null)
            Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", _oldExpiry);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidInput_CreatesUser()
    {
        var dto = new UserRegistrationDto
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "Pass1234"
        };

        await _service.RegisterUserAsync(dto);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        Assert.NotNull(user);
        Assert.Equal(dto.Email, user!.Email);
        Assert.Equal(dto.Username, user.Username);

        _mockLogger.Verify(l => l.LogInformation("Starting user registration for {Email}", dto.Email), Times.Once);
        _mockLogger.Verify(l => l.LogInformation("User {Email} registered successfully.", dto.Email), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateUsername_ThrowsException()
    {
        var dto = new UserRegistrationDto
        {
            Name = "John",
            Username = "duplicate",
            Email = "dup1@example.com",
            Password = "Pass1234"
        };
        await _service.RegisterUserAsync(dto);

        var duplicateDto = new UserRegistrationDto
        {
            Name = "Jane",
            Username = "duplicate",
            Email = "dup2@example.com",
            Password = "Pass5678"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterUserAsync(duplicateDto));
        _mockLogger.Verify(
            l => l.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error occurred during user registration")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateEmail_ThrowsException()
    {
        var dto = new UserRegistrationDto
        {
            Name = "John",
            Username = "johnny",
            Email = "same@example.com",
            Password = "Pass1234"
        };
        await _service.RegisterUserAsync(dto);

        var duplicateDto = new UserRegistrationDto
        {
            Name = "Jane",
            Username = "janey",
            Email = "same@example.com",
            Password = "Pass5678"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterUserAsync(duplicateDto));
        _mockLogger.Verify(
            l => l.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error occurred during user registration")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_PasswordIsHashed()
    {
        var dto = new UserRegistrationDto
        {
            Name = "Alice",
            Username = "alice123",
            Email = "alice@example.com",
            Password = "MySecret123"
        };

        await _service.RegisterUserAsync(dto);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        Assert.NotNull(user);
        Assert.NotEqual(dto.Password, user!.PasswordHash); 
        Assert.StartsWith("$", user.PasswordHash);
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