using Moq;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Facilities;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class UpdateFacilityValidatorTests
{
    private readonly Mock<IMaintenanceRepository> _maintenance;
    private readonly UpdateFacilityValidator _validator;

    public UpdateFacilityValidatorTests()
    {
        _maintenance = MockFactories.MaintenanceRepo();
        _validator = new UpdateFacilityValidator(_maintenance.Object);
    }

    // ------------------------------------------------------------
    // NAME VALIDATION
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameEmpty()
    {
        var dto = new UpdateFacilityDto { Name = "" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameTooLong()
    {
        var dto = new UpdateFacilityDto { Name = new string('a', 101) };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenNameValid()
    {
        var dto = new UpdateFacilityDto { Name = "Center Court" };

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    // ------------------------------------------------------------
    // TYPE VALIDATION
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenTypeTooLong()
    {
        var dto = new UpdateFacilityDto { Type = new string('x', 51) };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenTypeValid()
    {
        var dto = new UpdateFacilityDto { Type = "Basketball Court" };

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    // ------------------------------------------------------------
    // CAPACITY VALIDATION
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenCapacityNotPositive()
    {
        var dto = new UpdateFacilityDto { Capacity = 0 };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCapacityPositive()
    {
        var dto = new UpdateFacilityDto { Capacity = 30 };

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    // ------------------------------------------------------------
    // STATUS VALIDATION
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenStatusInvalid()
    {
        var dto = new UpdateFacilityDto { Status = "Exploded" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    [Theory]
    [InlineData("Available")]
    [InlineData("Unavailable")]
    [InlineData("UnderMaintenance")]
    public async Task Validate_ShouldPass_WhenStatusAllowed(string status)
    {
        var dto = new UpdateFacilityDto { Status = status };

        _maintenance
            .Setup(m => m.GetByFacilityAsync(ExistingFacility.Id))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    // ------------------------------------------------------------
    // MAINTENANCE BLOCK RULES
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenUnderMaintenanceAndTryingToMarkAvailable()
    {
        var existing = ExistingFacility;
        existing.Status = "UnderMaintenance";

        var dto = new UpdateFacilityDto { Status = "Available" };

        await ShouldThrowConflict(() => _validator.ValidateAsync(dto, existing));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenActiveMaintenanceBlocksAvailable()
    {
        var dto = new UpdateFacilityDto { Status = "Available" };

        var maintenance = new MaintenanceRecord { Status = "InProgress" };

        _maintenance
            .Setup(m => m.GetByFacilityAsync(ExistingFacility.Id))
            .ReturnsAsync(new[] { maintenance });

        await ShouldThrowConflict(() => _validator.ValidateAsync(dto, ExistingFacility));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenMarkingAvailableButNoActiveMaintenance()
    {
        var existing = ExistingFacility;
        existing.Status = "Unavailable";

        var dto = new UpdateFacilityDto { Status = "Available" };

        _maintenance
            .Setup(m => m.GetByFacilityAsync(existing.Id))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, existing));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenStatusNotProvided()
    {
        var dto = new UpdateFacilityDto();

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingFacility));
    }
}
