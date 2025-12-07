using Moq;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Maintenance;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class CreateMaintenanceValidatorTests
{
    private readonly Mock<IFacilityRepository> _facility;
    private readonly Mock<IMaintenanceRepository> _maintenance;

    private readonly CreateMaintenanceValidator _validator;

    public CreateMaintenanceValidatorTests()
    {
        _facility = MockFactories.FacilityRepo();
        _maintenance = MockFactories.MaintenanceRepo();

        _validator = new CreateMaintenanceValidator(_facility.Object, _maintenance.Object);
    }

    // ------------------------------------------------------------
    // INVALID FACILITY ID
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenFacilityIdInvalid()
    {
        var dto = FakeData.CreateMaintenanceDto().RuleFor(x => x.FacilityId, _ => 0).Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ------------------------------------------------------------
    // FACILITY NOT FOUND
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenFacilityNotFound()
    {
        var dto = FakeData.CreateMaintenanceDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        await ShouldThrowNotFound(() => _validator.ValidateAsync(dto));
    }

    // ------------------------------------------------------------
    // START >= END
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenStartAfterOrEqualEnd()
    {
        var dto = FakeData
            .CreateMaintenanceDto()
            .RuleFor(d => d.StartTime, _ => DateTime.UtcNow.AddHours(3))
            .RuleFor(d => d.EndTime, _ => DateTime.UtcNow.AddHours(2))
            .Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ------------------------------------------------------------
    // START IN THE PAST
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenStartInPast()
    {
        var dto = FakeData
            .CreateMaintenanceDto()
            .RuleFor(d => d.StartTime, _ => DateTime.UtcNow.AddHours(-1))
            .RuleFor(d => d.EndTime, _ => DateTime.UtcNow.AddHours(3))
            .Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        await ShouldThrowPastDate(() => _validator.ValidateAsync(dto));
    }

    // ------------------------------------------------------------
    // OVERLAP WITH EXISTING MAINTENANCE
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenOverlappingExistingMaintenance()
    {
        var dto = FakeData.CreateMaintenanceDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        var overlap = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.StartTime, _ => dto.StartTime.AddMinutes(-30))
            .RuleFor(m => m.EndTime, _ => dto.EndTime.AddMinutes(30))
            .Generate();

        _maintenance
            .Setup(m => m.GetByFacilityAsync(dto.FacilityId))
            .ReturnsAsync(new[] { overlap });

        await ShouldThrowConflict(() => _validator.ValidateAsync(dto));
    }

    // ------------------------------------------------------------
    // VALID CASE
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldPass_WhenNoConflicts()
    {
        var dto = FakeData.CreateMaintenanceDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        _maintenance
            .Setup(m => m.GetByFacilityAsync(dto.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }
}
