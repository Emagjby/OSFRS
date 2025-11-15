using Moq;
using OSFRS.Backend.Services;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Tests.Services;

public class MaintenanceServiceTests
{
    private readonly Mock<IMaintenanceRepository> _mockRepo;
    private readonly Mock<IFacilityRepository> _mockFacilityRepo;
    private readonly Mock<IAppLogger<MaintenanceService>> _mockLogger;
    private readonly IMaintenanceService _service;

    public MaintenanceServiceTests()
    {
        _mockRepo = new Mock<IMaintenanceRepository>();
        _mockFacilityRepo = new Mock<IFacilityRepository>();
        _mockLogger = new Mock<IAppLogger<MaintenanceService>>();

        _service = new MaintenanceService(
            _mockRepo.Object,
            _mockFacilityRepo.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ScheduleMaintenanceAsync_ShouldCreate_WhenValid()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 1,
            Description = "Test",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Status = "Scheduled"
        };

        _mockFacilityRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Facility { Id = 1 });

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceRecord>()))
            .ReturnsAsync((MaintenanceRecord m) => m);

        var result = await _service.ScheduleMaintenanceAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.FacilityId, result.FacilityId);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<MaintenanceRecord>()), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ScheduleMaintenanceAsync_ShouldThrow_WhenFacilityNotFound()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 999,
            Description = "Oops",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        _mockFacilityRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Facility?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ScheduleMaintenanceAsync(dto));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<MaintenanceRecord>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleMaintenanceAsync_ShouldThrow_WhenEndBeforeStart()
    {
        var dto = new CreateMaintenanceRecordDto
        {
            FacilityId = 1,
            Description = "Bad time",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(-10)
        };

        _mockFacilityRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Facility { Id = 1 });

        await Assert.ThrowsAsync<ArgumentException>(() => _service.ScheduleMaintenanceAsync(dto));
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldUpdate_WhenValid()
    {
        var existing = new MaintenanceRecord
        {
            Id = 1,
            FacilityId = 1,
            Description = "Old",
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        var dto = new UpdateMaintenanceRecordDto
        {
            Description = "Updated",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(3),
            Status = "InProgress"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceRecord>()))
            .ReturnsAsync((MaintenanceRecord m) => m);

        var updated = await _service.UpdateMaintenanceAsync(1, dto);

        Assert.Equal("Updated", updated!.Description);
        Assert.Equal(dto.Status, updated.Status);

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceRecord>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldThrow_WhenRecordNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((MaintenanceRecord?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateMaintenanceAsync(1, new UpdateMaintenanceRecordDto()));
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldThrow_WhenInvalidTimes()
    {
        var existing = new MaintenanceRecord { Id = 1 };

        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(-10)
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateMaintenanceAsync(1, dto));
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_ShouldReturnTrue_WhenDeleted()
    {
        _mockRepo.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteMaintenanceAsync(1);

        Assert.True(result);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_ShouldReturnFalse_WhenNotFound()
    {
        _mockRepo.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(false);

        var result = await _service.DeleteMaintenanceAsync(1);

        Assert.False(result);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetMaintenanceByFacilityAsync_ShouldReturnRecords_WhenFacilityExists()
    {
        _mockFacilityRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Facility { Id = 1 });

        _mockRepo.Setup(r => r.GetByFacilityAsync(1))
            .ReturnsAsync(new List<MaintenanceRecord> { new MaintenanceRecord() });

        var result = await _service.GetMaintenanceByFacilityAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetMaintenanceByFacilityAsync_ShouldThrow_WhenFacilityNotFound()
    {
        _mockFacilityRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Facility?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetMaintenanceByFacilityAsync(1));
    }

    [Fact]
    public async Task GetUpcomingMaintenanceAsync_ShouldReturnList()
    {
        _mockRepo.Setup(r => r.GetUpcomingAsync())
            .ReturnsAsync(new List<MaintenanceRecord> { new MaintenanceRecord() });

        var result = await _service.GetUpcomingMaintenanceAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task SyncFacilityStatusesAsync_ShouldMarkUnderMaintenance()
    {
        var now = DateTime.UtcNow;

        var record = new MaintenanceRecord
        {
            Id = 1,
            FacilityId = 1,
            StartTime = now.AddMinutes(-5),
            EndTime = now.AddMinutes(5)
        };

        var facility = new Facility { Id = 1, Status = "Available" };

        _mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<MaintenanceRecord> { record });

        _mockFacilityRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(facility);

        _mockFacilityRepo.Setup(r => r.UpdateAsync(It.IsAny<Facility>()))
            .ReturnsAsync((Facility f) => f);

        await _service.SyncFacilityStatusesAsync();

        _mockFacilityRepo.Verify(r => r.UpdateAsync(It.Is<Facility>(f => f.Status == "UnderMaintenance")), Times.Once);
    }

    [Fact]
    public async Task SyncFacilityStatusesAsync_ShouldSkip_WhenFacilityMissing()
    {
        var record = new MaintenanceRecord
        {
            Id = 1,
            FacilityId = 99,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow.AddMinutes(10)
        };

        _mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<MaintenanceRecord> { record });

        _mockFacilityRepo.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Facility?)null);

        await _service.SyncFacilityStatusesAsync();

        _mockFacilityRepo.Verify(r => r.UpdateAsync(It.IsAny<Facility>()), Times.Never);
    }
}