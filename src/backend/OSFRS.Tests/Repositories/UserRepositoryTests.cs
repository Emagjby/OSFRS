using OSFRS.Backend.Data;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces;
using OSFRS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace OSFRS.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly OSFRSDbContext _context;
    private readonly Mock<IAppLogger<UserRepository>> _mockLogger;
    private readonly IUserRepository _repo;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        _mockLogger = new Mock<IAppLogger<UserRepository>>();
        _repo = new UserRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task AddUserAsync_ShouldSaveUser()
    {
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User",
            Name = "John Doe",
            PasswordHash = "hashedpw"
        };
        await _repo.AddUserAsync(user);

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("johndoe", savedUser!.Username);
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_ShouldReturnCorrectUser()
    {
        var user = new User {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User",
            Name = "John Doe",
            PasswordHash = "hashedpw"
        };
        await _repo.AddUserAsync(user);

        var byUsername = await _repo.GetByUsernameOrEmailAsync("johndoe");
        var byEmail = await _repo.GetByUsernameOrEmailAsync("john@example.com");

        Assert.Equal(user.Id, byUsername!.Id);
        Assert.Equal(user.Id, byEmail!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectUser()
    {
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User",
            Name = "John Doe",
            PasswordHash = "hashedpw"
        };
        await _repo.AddUserAsync(user);

        var result = await _repo.GetByIdAsync(1);
        Assert.Equal("johndoe", result!.Username);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUser()
    {
        var user = new User {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User",
            Name = "John Doe",
            PasswordHash = "hashedpw"
        };
        await _repo.AddUserAsync(user);

        user.Username = "johnny";
        await _repo.UpdateUserAsync(user);

        var updated = await _repo.GetByIdAsync(1);
        Assert.Equal("johnny", updated!.Username);
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldDetectDuplicateEmail()
    {
        var user = new User {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User",
            Name = "John Doe",
            PasswordHash = "hashedpw"
        };
        await _repo.AddUserAsync(user);

        Assert.True(await _repo.EmailExistsAsync("john@example.com"));
        Assert.False(await _repo.EmailExistsAsync("noone@example.com"));
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldDetectDuplicateUsername()
    {
        var user = new User {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            Role = "User",
            Name = "John Doe",
            PasswordHash = "hashedpw"
        };
        await _repo.AddUserAsync(user);

        Assert.True(await _repo.UsernameExistsAsync("johndoe"));
        Assert.False(await _repo.UsernameExistsAsync("janedoe"));
    }
}