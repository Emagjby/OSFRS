using Moq;
using OSFRS.Backend.Services;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Tests.Services;

public class FacilityServiceTests
{
    private readonly Mock<IFacilityRepository> _mockRepo;
    private readonly Mock<IAppLogger<FacilityService>> _mockLogger;
    private readonly IFacilityService _service;

    public FacilityServiceTests()
    {
        _mockRepo = new Mock<IFacilityRepository>();
        _mockLogger = new Mock<IAppLogger<FacilityService>>();
        _service = new FacilityService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateFacilityAsync_ShouldCreate_WhenValid()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Court 1",
            Type = "Tennis",
            Capacity = 4,
            Status = "Available"
        };

        var facility = new Facility { Id = 1, Name = dto.Name };

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Facility>()))
             .ReturnsAsync(facility);

        var result = await _service.CreateFacilityAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Court 1", result.Name);

        _mockRepo.Verify(r =>
            r.AddAsync(It.Is<Facility>(f => f.Name == "Court 1")),
            Times.Once
        );

        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateFacilityAsync_ShouldThrow_WhenInvalid()
    {
        var dto = new CreateFacilityDto
        {
            Name = "",
            Type = "",
            Capacity = 0,
            Status = ""
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateFacilityAsync(dto));

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Facility>()), Times.Never);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFacilityAsync_ShouldReturnTrue_WhenExists()
    {
        _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteFacilityAsync(1);

        Assert.True(result);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), 1), Times.Once);
    }

    [Fact]
    public async Task DeleteFacilityAsync_ShouldReturnFalse_WhenNotFound()
    {
        _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _service.DeleteFacilityAsync(1);

        Assert.False(result);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), 1), Times.Once);
    }

    [Fact]
    public async Task GetAllFacilitiesAsync_ShouldReturnFacilities()
    {
        var list = new List<Facility> { new Facility { Id = 1 }, new Facility { Id = 2 } };

        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

        var result = await _service.GetAllFacilitiesAsync();

        Assert.Equal(2, result.Count());
        _mockLogger.Verify(l => l.LogInformation("Retrieved {Count} facilities.", 2), Times.Once);
    }

    [Fact]
    public async Task GetAllFacilitiesAsync_ShouldReturnEmpty_WhenNoFacilities()
    {
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Facility>());

        var result = await _service.GetAllFacilitiesAsync();

        Assert.Empty(result);
        _mockLogger.Verify(l => l.LogInformation("Retrieved {Count} facilities.", 0), Times.Once);
    }

    [Fact]
    public async Task GetFacilityByIdAsync_ShouldReturnFacility_WhenExists()
    {
        var facility = new Facility { Id = 1 };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(facility);

        var result = await _service.GetFacilityByIdAsync(1);

        Assert.NotNull(result);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetFacilityByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Facility?)null);

        var result = await _service.GetFacilityByIdAsync(1);

        Assert.Null(result);
        _mockLogger.Verify(l => l.LogWarning("Facility with ID {Id} not found.", 1), Times.Once);
    }

    [Fact]
    public async Task IsFacilityAvailableAsync_ShouldReturnRepoValue()
    {
        _mockRepo.Setup(r => r.IsFacilityAvailableAsync(1)).ReturnsAsync(true);

        var result = await _service.IsFacilityAvailableAsync(1);

        Assert.True(result);
        _mockRepo.Verify(r => r.IsFacilityAvailableAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldUpdate_WhenExists()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1))
             .ReturnsAsync(new Facility { Id = 1 });

        var result = await _service.UpdateAvailabilityAsync(1, true);

        Assert.True(result);

        _mockRepo.Verify(r => r.UpdateAvailabilityAsync(1, true), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), 1, "Available"), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldThrow_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Facility?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAvailabilityAsync(1, true)
        );

        _mockRepo.Verify(r => r.UpdateAvailabilityAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFacilityAsync_ShouldUpdate_WhenValid()
    {
        var existing = new Facility
        {
            Id = 1,
            Name = "Old",
            Type = "Basketball",
            Capacity = 5,
            Status = "Available"
        };

        var dto = new UpdateFacilityDto
        {
            Name = "New Name",
            Capacity = 10
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Facility>()))
             .ReturnsAsync((Facility f) => f);

        var updated = await _service.UpdateFacilityAsync(1, dto);

        Assert.Equal("New Name", updated!.Name);
        Assert.Equal(10, updated.Capacity);

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Facility>()), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), updated.Name, updated.Id), Times.Once);
    }

    [Fact]
    public async Task UpdateFacilityAsync_ShouldThrow_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Facility?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateFacilityAsync(1, new UpdateFacilityDto())
        );
    }

    [Fact]
    public async Task UpdateFacilityAsync_ShouldThrow_WhenValidationFails()
    {
        var existing = new Facility { Id = 1 };
        var dto = new UpdateFacilityDto { Name = "", Type = "", Status = "", Capacity = null };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateFacilityAsync(1, dto)
        );

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Facility>()), Times.Never);
    }
}