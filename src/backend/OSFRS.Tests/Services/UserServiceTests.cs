using Microsoft.EntityFrameworkCore;
using Moq;
using OSFRS.Backend.Data;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.DTOs;

namespace OSFRS.Tests.Services;

public class UserServiceTests
{
    private readonly OSFRSDbContext _context;
    private readonly UserRepository _repo;
    private readonly PasswordHasher _hasher;
    private readonly Mock<IAppLogger<UserService>> _mockLogger;
    private readonly UserService _service;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        var mockRepoLogger = new Mock<IAppLogger<UserRepository>>();
        _repo = new UserRepository(_context, mockRepoLogger.Object);
        _hasher = new PasswordHasher();
        _mockLogger = new Mock<IAppLogger<UserService>>();
        _service = new UserService(_repo, _hasher, _mockLogger.Object);
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
}