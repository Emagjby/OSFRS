using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Tests.Services;

public class ProfileServiceTests
{
    private readonly OSFRSDbContext _context;
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly Mock<IAppLogger<ProfileService>> _mockLogger;
    private readonly IProfileService _service;

    public ProfileServiceTests()
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        var mockRepoLogger = new Mock<IAppLogger<UserRepository>>();
        _repo = new UserRepository(_context, mockRepoLogger.Object);
        _hasher = new PasswordHasher();
        _mockLogger = new Mock<IAppLogger<ProfileService>>();
        _service = new ProfileService(_repo, _hasher, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsCorrectUserData()
    {
        var user = new User
        {
            Name = "John Doe",
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = _hasher.Hash("Pass1234"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user);

        var result = await _service.GetProfileAsync(user.Id);

        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Role, result.Role);

        _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Profile fetched"))), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidData_UpdatesUser()
    {
        var user = new User
        {
            Name = "Jane Doe",
            Username = "janedoe",
            Email = "jane@example.com",
            PasswordHash = _hasher.Hash("Pass1234"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user);

        var dto = new UpdatedProfileDto
        {
            Name = "Jane Updated",
            Username = "janeupdated",
            Email = "updated@example.com",
            Password = "NewPass123"
        };

        await _service.UpdateProfileAsync(user.Id, dto);

        _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Starting profile update"))), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Profile updated successfully"))), Times.Once);

        var updated = await _repo.GetByIdAsync(user.Id);

        Assert.Equal("Jane Updated", updated!.Name);
        Assert.Equal("janeupdated", updated.Username);
        Assert.Equal("updated@example.com", updated.Email);
        Assert.True(_hasher.Verify("NewPass123", updated.PasswordHash));
    }

    [Fact]
    public async Task UpdateProfileAsync_DuplicateUsername_ThrowsException()
    {
        var user1 = new User
        {
            Name = "User1",
            Username = "user1",
            Email = "user1@example.com",
            PasswordHash = _hasher.Hash("Pass1234"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var user2 = new User
        {
            Name = "User2",
            Username = "user2",
            Email = "user2@example.com",
            PasswordHash = _hasher.Hash("Pass5678"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user1);
        await _repo.AddUserAsync(user2);

        var dto = new UpdatedProfileDto
        {
            Name = "New Name",
            Username = "user1",
            Email = "user2@example.com"
        };

        await Assert.ThrowsAsync<Exception>(() => _service.UpdateProfileAsync(user2.Id, dto));

        _mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Profile update failed"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateProfileAsync_DuplicateEmail_ThrowsException()
    {
        var user1 = new User
        {
            Name = "User1",
            Username = "user1",
            Email = "user1@example.com",
            PasswordHash = _hasher.Hash("Pass1234"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var user2 = new User
        {
            Name = "User2",
            Username = "user2",
            Email = "user2@example.com",
            PasswordHash = _hasher.Hash("Pass5678"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user1);
        await _repo.AddUserAsync(user2);

        var dto = new UpdatedProfileDto
        {
            Name = "New Name",
            Username = "user2",
            Email = "user1@example.com"
        };

        await Assert.ThrowsAsync<Exception>(() => _service.UpdateProfileAsync(user2.Id, dto));

        _mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Profile update failed"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateProfileAsync_PasswordIsHashed_WhenChanged()
    {
        var user = new User
        {
            Name = "Bob",
            Username = "bob",
            Email = "bob@example.com",
            PasswordHash = _hasher.Hash("OldPass123"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddUserAsync(user);

        var dto = new UpdatedProfileDto
        {
            Name = "Bob Updated",
            Username = "bob",
            Email = "bob@example.com",
            Password = "NewPass123"
        };

        await _service.UpdateProfileAsync(user.Id, dto);
        var updated = await _repo.GetByIdAsync(user.Id);

        Assert.True(_hasher.Verify("NewPass123", updated!.PasswordHash));
    }

    
}