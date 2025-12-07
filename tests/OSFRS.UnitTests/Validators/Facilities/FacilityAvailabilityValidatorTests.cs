using Moq;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Facilities;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class FacilityAvailabilityValidatorTests
{
    private readonly Mock<IMaintenanceRepository> _maintenance;
    private readonly FacilityAvailabilityValidator _validator;

    public FacilityAvailabilityValidatorTests()
    {
        _maintenance = MockFactories.MaintenanceRepo();
        _validator = new FacilityAvailabilityValidator(_maintenance.Object);
    }

    // ------------------------------------------------------------
    // FACILITY EXISTS
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenFacilityNull()
    {
        Facility? facility = null;

        await ShouldThrowNotFound(() =>
            _validator.ValidateAsync((facility!, newAvailability: true))
        );
    }

    // ------------------------------------------------------------
    // ACTIVE MAINTENANCE BLOCKS AVAILABILITY
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenMarkingAvailableDuringActiveMaintenance()
    {
        var maintenance = new MaintenanceRecord
        {
            FacilityId = ExistingCourt.Id,
            Status = "InProgress",
        };

        _maintenance
            .Setup(m => m.GetByFacilityAsync(ExistingCourt.Id))
            .ReturnsAsync(new[] { maintenance });

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((ExistingCourt, newAvailability: true))
        );
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenAnyMaintenanceIsInProgress()
    {
        var records = new[]
        {
            new MaintenanceRecord { Status = "Scheduled" },
            new MaintenanceRecord { Status = "InProgress" }, // triggers the block
            new MaintenanceRecord { Status = "Completed" },
        };

        _maintenance.Setup(m => m.GetByFacilityAsync(ExistingCourt.Id)).ReturnsAsync(records);

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((ExistingCourt, newAvailability: true))
        );
    }

    // ------------------------------------------------------------
    // AVAILABLE WITH NO ACTIVE MAINTENANCE
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldPass_WhenNoMaintenanceActive()
    {
        _maintenance
            .Setup(m => m.GetByFacilityAsync(ExistingCourt.Id))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((ExistingCourt, newAvailability: true))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenMaintenanceNotInProgress()
    {
        var records = new[]
        {
            new MaintenanceRecord { Status = "Scheduled" },
            new MaintenanceRecord { Status = "Completed" },
        };

        _maintenance.Setup(m => m.GetByFacilityAsync(ExistingCourt.Id)).ReturnsAsync(records);

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((ExistingCourt, newAvailability: true))
        );
    }

    // ------------------------------------------------------------
    // SETTING UNAVAILABLE ALWAYS ALLOWED
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldPass_WhenSettingUnavailable_EvenWithActiveMaintenance()
    {
        var records = new[] { new MaintenanceRecord { Status = "InProgress" } };

        _maintenance.Setup(m => m.GetByFacilityAsync(ExistingCourt.Id)).ReturnsAsync(records);

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((ExistingCourt, newAvailability: false))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenSettingUnavailable_NoMaintenance()
    {
        _maintenance
            .Setup(m => m.GetByFacilityAsync(ExistingCourt.Id))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((ExistingCourt, newAvailability: false))
        );
    }
}
