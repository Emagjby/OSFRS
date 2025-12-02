using FluentAssertions;
using Moq;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class MaintenanceServiceTests
{
    private readonly Mock<IMaintenanceRepository> _repo;
    private readonly Mock<IFacilityRepository> _facilityRepo;
    private readonly Mock<IAppLogger<MaintenanceService>> _logger;

    private readonly Mock<IValidator<CreateMaintenanceRecordDto>> _createValidator;
    private readonly Mock<
        IUpdateValidator<UpdateMaintenanceRecordDto, MaintenanceRecord>
    > _updateValidator;

    private readonly MaintenanceService _service;

    public MaintenanceServiceTests()
    {
        _repo = MockFactories.MaintenanceRepo();
        _facilityRepo = MockFactories.FacilityRepo();
        _logger = MockFactories.Logger<MaintenanceService>();

        _createValidator = MockFactories.Validator<CreateMaintenanceRecordDto>();
        _updateValidator = MockFactories.UpdateValidator<
            UpdateMaintenanceRecordDto,
            MaintenanceRecord
        >();

        _service = new MaintenanceService(
            _repo.Object,
            _facilityRepo.Object,
            _logger.Object,
            _createValidator.Object,
            _updateValidator.Object
        );
    }

    // ============================================================
    // CREATE -> Validator is called
    // ============================================================

    [Fact]
    public async Task Schedule_ShouldCallCreateValidator()
    {
        var dto = FakeData.CreateMaintenanceDto().Generate();

        _facilityRepo
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Facility { Id = dto.FacilityId });

        await _service.ScheduleMaintenanceAsync(dto);

        _createValidator.Verify(v => v.ValidateAsync(dto), Times.Once);
    }

    // ============================================================
    // CREATE -> throws NotFound when facility missing
    // ============================================================

    [Fact]
    public async Task Schedule_ShouldThrowNotFound_WhenFacilityMissing()
    {
        var dto = FakeData.CreateMaintenanceDto().Generate();

        _facilityRepo
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var act = async () => await _service.ScheduleMaintenanceAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // CREATE -> repository Add + Save
    // ============================================================

    [Fact]
    public async Task Schedule_ShouldAddRecord_AndSave()
    {
        var dto = FakeData.CreateMaintenanceDto().Generate();

        _facilityRepo
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Facility { Id = dto.FacilityId });

        await _service.ScheduleMaintenanceAsync(dto);

        _repo.Verify(
            r => r.AddAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // UPDATE -> throws NotFound
    // ============================================================

    [Fact]
    public async Task Update_ShouldThrowNotFound_WhenRecordMissing()
    {
        var dto = FakeData.UpdateMaintenanceDto().Generate();

        var INVALID_ID = 999;

        _repo
            .Setup(r => r.GetByIdAsync(INVALID_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        var act = async () => await _service.UpdateMaintenanceAsync(INVALID_ID, dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // UPDATE -> calls validator with correct args
    // ============================================================

    [Fact]
    public async Task Update_ShouldCallUpdateValidator()
    {
        var existing = FakeData.MaintenanceRecord().Generate();
        var dto = FakeData.UpdateMaintenanceDto().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _service.UpdateMaintenanceAsync(existing.Id, dto);

        _updateValidator.Verify(v => v.ValidateAsync(dto, existing), Times.Once);
    }

    // ============================================================
    // UPDATE -> modifies only provided fields
    // ============================================================

    [Fact]
    public async Task Update_ShouldModifyOnlyProvidedFields()
    {
        var existing = FakeData.MaintenanceRecord().Generate();
        existing.Description = "OldDesc";

        var dto = new UpdateMaintenanceRecordDto
        {
            Description = "NewDesc",
            StartTime = null, // stays same
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        _repo
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var updated = await _service.UpdateMaintenanceAsync(existing.Id, dto);

        updated!.Description.Should().Be("NewDesc");
        updated.EndTime.Should().Be(dto.EndTime);
        updated.StartTime.Should().Be(existing.StartTime);

        _repo.Verify(r => r.Update(existing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // FACILITY EXISTS check
    // ============================================================

    [Fact]
    public async Task GetMaintenanceByFacility_ShouldThrowNotFound_WhenFacilityMissing()
    {
        var MISSING_ID = 50;

        _facilityRepo
            .Setup(f => f.GetByIdAsync(MISSING_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var act = async () => await _service.GetMaintenanceByFacilityAsync(MISSING_ID);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // DELETE
    // ============================================================

    [Fact]
    public async Task Delete_ShouldReturnFalse_WhenNotFound()
    {
        var INVALID_ID = 999;

        _repo
            .Setup(r => r.GetByIdAsync(INVALID_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        var result = await _service.DeleteMaintenanceAsync(INVALID_ID);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ShouldRemoveAndSave_WhenFound()
    {
        var record = FakeData.MaintenanceRecord().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var result = await _service.DeleteMaintenanceAsync(record.Id);

        result.Should().BeTrue();
        _repo.Verify(r => r.Remove(record), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // SYNC STATUSES (big one)
    // ============================================================

    [Fact]
    public async Task Sync_ShouldMarkCompleted_WhenEndBeforeNow()
    {
        var now = DateTime.UtcNow;

        var record = FakeData
            .MaintenanceRecord()
            .RuleFor(r => r.EndTime, _ => now.AddHours(-1))
            .RuleFor(r => r.StartTime, _ => now.AddHours(-3))
            .RuleFor(r => r.Status, _ => "Scheduled")
            .Generate();

        _repo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { record });

        _facilityRepo
            .Setup(f => f.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Facility>());

        await _service.SyncStatusesAsync();

        record.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Sync_ShouldMarkInProgress_WhenNowBetweenStartAndEnd()
    {
        var now = DateTime.UtcNow;

        var FACILITY_ID = 1;

        var facility = FakeData
            .Facility()
            .RuleFor(f => f.Id, _ => FACILITY_ID)
            .RuleFor(f => f.Status, _ => "Available")
            .Generate();

        var record = FakeData
            .MaintenanceRecord()
            .RuleFor(f => f.FacilityId, _ => FACILITY_ID)
            .RuleFor(r => r.StartTime, _ => now.AddHours(-1))
            .RuleFor(r => r.EndTime, _ => now.AddHours(2))
            .RuleFor(r => r.Status, _ => "Scheduled")
            .Generate();

        _repo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { record });
        _facilityRepo
            .Setup(f => f.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { facility });

        await _service.SyncStatusesAsync();

        record.Status.Should().Be("InProgress");
        facility.Status.Should().Be("UnderMaintenance");
    }

    [Fact]
    public async Task Sync_ShouldMarkScheduled_WhenStartAfterNow()
    {
        var now = DateTime.UtcNow;

        var record = FakeData
            .MaintenanceRecord()
            .RuleFor(r => r.StartTime, _ => now.AddHours(3))
            .RuleFor(r => r.EndTime, _ => now.AddHours(5))
            .RuleFor(r => r.Status, _ => "Pending")
            .Generate();

        _repo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { record });
        _facilityRepo
            .Setup(f => f.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Facility>());

        await _service.SyncStatusesAsync();

        record.Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task Sync_ShouldNotChangeCancelled()
    {
        var record = FakeData
            .MaintenanceRecord()
            .RuleFor(r => r.Status, _ => "Cancelled")
            .Generate();

        _repo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { record });
        _facilityRepo
            .Setup(f => f.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Facility>());

        await _service.SyncStatusesAsync();

        record.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Sync_ShouldRevertFacilityToAvailable_WhenNoInProgress()
    {
        var facility = FakeData
            .Facility()
            .RuleFor(f => f.Status, _ => "UnderMaintenance")
            .Generate();

        _repo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());
        _facilityRepo
            .Setup(f => f.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { facility });

        await _service.SyncStatusesAsync();

        facility.Status.Should().Be("Available");
    }
}
