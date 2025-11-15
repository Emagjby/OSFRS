using Microsoft.EntityFrameworkCore;
using Moq;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;

namespace OSFRS.Tests.Repositories;

public class FacilityRepositoryTests
{
    private readonly IFacilityRepository _repo;
    private readonly OSFRSDbContext _context;
    private readonly Mock<IAppLogger<FacilityRepository>> _mockLogger;

    public FacilityRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        _mockLogger = new Mock<IAppLogger<FacilityRepository>>();
        _repo = new FacilityRepository(_context, _mockLogger.Object);
    }

    private Facility CreateFacility(
        string name = "Court A",
        string type = "Basketball",
        int capacity = 20,
        string status = "Available")
    {
        return new Facility
        {
            Name = name,
            Type = type,
            Capacity = capacity,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task AddAsync_ValidFacility_AddsAndReturnsFacility()
    {
        var facility = CreateFacility();

        var result = await _repo.AddAsync(facility);

        Assert.NotNull(result);
        Assert.Equal(1, await _context.Facilities.CountAsync());
        Assert.Equal("Court A", result.Name);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        var facility = CreateFacility();
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var result = await _repo.DeleteAsync(facility.Id);

        Assert.True(result);
        Assert.Equal(0, _context.Facilities.Count());
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ReturnsFalse()
    {
        var result = await _repo.DeleteAsync(123);

        Assert.False(result);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_NoFacilities_ReturnsEmpty()
    {
        var result = await _repo.GetAllAsync();

        Assert.Empty(result);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetAllAsync_WithFacilities_ReturnsAll()
    {
        _context.Facilities.Add(CreateFacility("Court A"));
        _context.Facilities.Add(CreateFacility("Court B"));
        await _context.SaveChangesAsync();

        var result = await _repo.GetAllAsync();

        Assert.Equal(2, result.Count());
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsFacility()
    {
        var facility = CreateFacility();
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(facility.Id);

        Assert.NotNull(result);
        Assert.Equal(facility.Id, result!.Id);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), facility.Id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNullAndLogsWarning()
    {
        var result = await _repo.GetByIdAsync(999);

        Assert.Null(result);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), 999), Times.Once);
    }

    [Fact]
    public async Task IsFacilityAvailable_AvailableStatus_ReturnsTrue()
    {
        var facility = CreateFacility(status: "Available");
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var result = await _repo.IsFacilityAvailableAsync(facility.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task IsFacilityAvailable_UnavailableStatus_ReturnsFalse()
    {
        var facility = CreateFacility(status: "Unavailable");
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var result = await _repo.IsFacilityAvailableAsync(facility.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task IsFacilityAvailable_FacilityNotFound_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.IsFacilityAvailableAsync(100));
    }

    [Fact]
    public async Task UpdateAsync_ValidFacility_UpdatesAndReturnsUpdated()
    {
        var existing = CreateFacility();
        _context.Facilities.Add(existing);
        await _context.SaveChangesAsync();

        var updated = new Facility
        {
            Id = existing.Id,
            Name = "Updated Name",
            Type = "Tennis",
            Capacity = 5,
            Status = "Unavailable"
        };

        var result = await _repo.UpdateAsync(updated);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Tennis", result.Type);
        Assert.Equal(5, result.Capacity);
        Assert.Equal("Unavailable", result.Status);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_Throws()
    {
        var updated = new Facility
        {
            Id = 999,
            Name = "Nope"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.UpdateAsync(updated));
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), 999), Times.Once);
    }

    [Fact]
    public async Task UpdateAvailability_SetTrue_UpdatesStatusAvailable()
    {
        var facility = CreateFacility(status: "Unavailable");
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        await _repo.UpdateAvailabilityAsync(facility.Id, true);

        var result = await _context.Facilities.FindAsync(facility.Id);
        Assert.Equal("Available", result!.Status);
    }

    [Fact]
    public async Task UpdateAvailability_SetFalse_UpdatesStatusUnavailable()
    {
        var facility = CreateFacility(status: "Available");
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        await _repo.UpdateAvailabilityAsync(facility.Id, false);

        var result = await _context.Facilities.FindAsync(facility.Id);
        Assert.Equal("Unavailable", result!.Status);
    }

    [Fact]
    public async Task UpdateAvailability_FacilityNotFound_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.UpdateAvailabilityAsync(999, true));
    }
}