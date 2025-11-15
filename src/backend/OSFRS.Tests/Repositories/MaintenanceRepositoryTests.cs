using Microsoft.EntityFrameworkCore;
using Moq;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;

namespace OSFRS.Tests.Repositories;

public class MaintenanceRepositoryTests
{
    private readonly IMaintenanceRepository _repo;
    private readonly OSFRSDbContext _context;
    private readonly Mock<IAppLogger<MaintenanceRepository>> _mockLogger;

    public MaintenanceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        _mockLogger = new Mock<IAppLogger<MaintenanceRepository>>();
        _repo = new MaintenanceRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRecord()
    {
        var record = new MaintenanceRecord
        {
            FacilityId = 1,
            Description = "Fix lights",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repo.AddAsync(record);

        Assert.NotNull(result);
        Assert.Equal(1, _context.MaintenanceRecords.Count());
        Assert.Equal("Fix lights", result.Description);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRecord_WhenExists()
    {
        var record = new MaintenanceRecord
        {
            FacilityId = 1,
            Description = "Cleaning"
        };

        _context.MaintenanceRecords.Add(record);
        await _context.SaveChangesAsync();

        var result = await _repo.DeleteAsync(record.Id);

        Assert.True(result);
        Assert.Empty(_context.MaintenanceRecords);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _repo.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRecords()
    {
        _context.MaintenanceRecords.AddRange(
            new MaintenanceRecord { FacilityId = 1, StartTime = DateTime.UtcNow },
            new MaintenanceRecord { FacilityId = 2, StartTime = DateTime.UtcNow.AddHours(1) }
        );
        await _context.SaveChangesAsync();

        var result = await _repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _repo.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByFacilityAsync_ShouldReturnCorrectRecords()
    {
        _context.MaintenanceRecords.AddRange(
            new MaintenanceRecord { FacilityId = 5, Description = "A" },
            new MaintenanceRecord { FacilityId = 5, Description = "B" },
            new MaintenanceRecord { FacilityId = 7, Description = "C" }
        );
        await _context.SaveChangesAsync();

        var result = await _repo.GetByFacilityAsync(5);

        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal(5, r.FacilityId));
    }

    [Fact]
    public async Task GetByFacilityAsync_ShouldReturnEmpty_WhenNotFound()
    {
        var result = await _repo.GetByFacilityAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRecord_WhenExists()
    {
        var record = new MaintenanceRecord
        {
            FacilityId = 3,
            Description = "Wash court"
        };

        _context.MaintenanceRecords.Add(record);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(record.Id);

        Assert.NotNull(result);
        Assert.Equal("Wash court", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync(12345);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUpcomingAsync_ShouldReturnOnlyFuture()
    {
        _context.MaintenanceRecords.AddRange(
            new MaintenanceRecord { FacilityId = 1, StartTime = DateTime.UtcNow.AddHours(2) },
            new MaintenanceRecord { FacilityId = 2, StartTime = DateTime.UtcNow.AddHours(-2) }
        );
        await _context.SaveChangesAsync();

        var result = await _repo.GetUpcomingAsync();

        Assert.Single(result);
        Assert.True(result.First().StartTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task GetUpcomingAsync_ShouldReturnEmpty_WhenNoneUpcoming()
    {
        _context.MaintenanceRecords.Add(
            new MaintenanceRecord { FacilityId = 1, StartTime = DateTime.UtcNow.AddHours(-10) }
        );
        await _context.SaveChangesAsync();

        var result = await _repo.GetUpcomingAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRecord_WhenExists()
    {
        var record = new MaintenanceRecord
        {
            FacilityId = 1,
            Description = "Old",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Status = "Scheduled"
        };

        _context.MaintenanceRecords.Add(record);
        await _context.SaveChangesAsync();

        var updated = new MaintenanceRecord
        {
            Id = record.Id,
            FacilityId = 1,
            Description = "Updated desc",
            StartTime = record.StartTime.AddHours(1),
            EndTime = record.EndTime.AddHours(1),
            Status = "InProgress"
        };

        var result = await _repo.UpdateAsync(updated);

        Assert.Equal("Updated desc", result.Description);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenRecordNotFound()
    {
        var updated = new MaintenanceRecord
        {
            Id = 999,
            FacilityId = 100
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.UpdateAsync(updated)
        );
    }
}