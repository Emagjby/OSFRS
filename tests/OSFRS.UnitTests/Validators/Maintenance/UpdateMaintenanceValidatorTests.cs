using Moq;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Maintenance;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class UpdateMaintenanceValidatorTests
{
    private readonly Mock<IMaintenanceRepository> _repo;
    private readonly UpdateMaintenanceValidator _validator;

    public UpdateMaintenanceValidatorTests()
    {
        _repo = MockFactories.MaintenanceRepo();
        _validator = new UpdateMaintenanceValidator(_repo.Object);
    }

    // ------------------------------------------------------------
    // NOT FOUND
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenExistingIsNull()
    {
        var dto = new UpdateMaintenanceRecordDto();
        MaintenanceRecord? existing = null!;

        await ShouldThrowNotFound(() => _validator.ValidateAsync(dto, existing));
    }

    // ------------------------------------------------------------
    // PAST MAINTENANCE CANNOT BE MODIFIED
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenExistingIsPast()
    {
        var dto = new UpdateMaintenanceRecordDto();

        var existing = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.EndTime, _ => DateTime.UtcNow.AddHours(-1))
            .Generate();

        await ShouldThrowConflict(() => _validator.ValidateAsync(dto, existing));
    }

    // ------------------------------------------------------------
    // TIME WINDOW VALIDATION
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenNewStartAfterEnd()
    {
        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(3),
        };

        var existing = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.EndTime, _ => DateTime.UtcNow.AddHours(10))
            .Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, existing));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenStartProvidedInPast()
    {
        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(5),
        };

        var existing = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.EndTime, _ => DateTime.UtcNow.AddHours(10))
            .Generate();

        await ShouldThrowPastDate(() => _validator.ValidateAsync(dto, existing));
    }

    // ------------------------------------------------------------
    // PARTIAL UPDATES (fallback to existing values)
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldPass_WhenOnlyDescriptionProvided()
    {
        var dto = new UpdateMaintenanceRecordDto { Description = "Update text only" };

        var existing = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.StartTime, _ => DateTime.UtcNow.AddHours(1))
            .RuleFor(m => m.EndTime, _ => DateTime.UtcNow.AddHours(5))
            .Generate();

        _repo
            .Setup(r => r.GetByFacilityAsync(existing.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, existing));
    }

    // ------------------------------------------------------------
    // OVERLAP DETECTION
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenUpdatedWindowOverlapsOtherMaintenance()
    {
        var existing = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.StartTime, _ => DateTime.UtcNow.AddHours(1))
            .RuleFor(m => m.EndTime, _ => DateTime.UtcNow.AddHours(5))
            .Generate();

        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = existing.StartTime.AddMinutes(-30),
            EndTime = existing.EndTime.AddMinutes(30),
        };

        var overlap = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.Id, _ => existing.Id + 1) // other record
            .RuleFor(m => m.StartTime, _ => existing.StartTime.AddMinutes(-15))
            .RuleFor(m => m.EndTime, _ => existing.EndTime.AddMinutes(15))
            .Generate();

        _repo.Setup(r => r.GetByFacilityAsync(existing.FacilityId)).ReturnsAsync(new[] { overlap });

        await ShouldThrowConflict(() => _validator.ValidateAsync(dto, existing));
    }

    // ------------------------------------------------------------
    // VALID CASE
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldPass_WhenNoConflicts()
    {
        var existing = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.StartTime, _ => DateTime.UtcNow.AddHours(1))
            .RuleFor(m => m.EndTime, _ => DateTime.UtcNow.AddHours(5))
            .Generate();

        var dto = new UpdateMaintenanceRecordDto
        {
            StartTime = existing.StartTime.AddHours(1),
            EndTime = existing.EndTime.AddHours(1),
        };

        _repo
            .Setup(r => r.GetByFacilityAsync(existing.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, existing));
    }
}
