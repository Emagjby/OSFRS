using Moq;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Reservations;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class CreateReservationValidatorTests
{
    private readonly Mock<IFacilityRepository> _facility;
    private readonly Mock<IReservationRepository> _reservation;
    private readonly Mock<IMaintenanceRepository> _maintenance;

    private readonly CreateReservationValidator _validator;

    public CreateReservationValidatorTests()
    {
        _facility = MockFactories.FacilityRepo();
        _reservation = MockFactories.ReservationRepo();
        _maintenance = MockFactories.MaintenanceRepo();

        _validator = new CreateReservationValidator(
            _facility.Object,
            _reservation.Object,
            _maintenance.Object
        );
    }

    // ------------------------------------------------------------
    // 1. INVALID FACILITY ID
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenFacilityIdInvalid()
    {
        var dto = FakeData.CreateReservationDto().RuleFor(x => x.FacilityId, _ => 0).Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync((dto, 10)));
    }

    // ------------------------------------------------------------
    // 2. FACILITY NOT FOUND
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenFacilityNotFound()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        await ShouldThrowNotFound(() => _validator.ValidateAsync((dto, 10)));
    }

    // ------------------------------------------------------------
    // 3. START >= END
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenStartNotBeforeEnd()
    {
        var dto = FakeData
            .CreateReservationDto()
            .RuleFor(d => d.StartTime, _ => DateTime.UtcNow.AddHours(2))
            .RuleFor(d => d.EndTime, _ => DateTime.UtcNow.AddHours(1))
            .Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        await ShouldThrowValidation(() => _validator.ValidateAsync((dto, 10)));
    }

    // ------------------------------------------------------------
    // 4. START IN THE PAST
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenStartInPast()
    {
        var dto = FakeData
            .CreateReservationDto()
            .RuleFor(d => d.StartTime, _ => DateTime.UtcNow.AddHours(-1))
            .RuleFor(d => d.EndTime, _ => DateTime.UtcNow.AddHours(1))
            .Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        await ShouldThrowPastDate(() => _validator.ValidateAsync((dto, 10)));
    }

    // ------------------------------------------------------------
    // 5. MAINTENANCE OVERLAP
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenOverlapsMaintenance()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        var maintenance = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.StartTime, _ => dto.StartTime.AddMinutes(-30))
            .RuleFor(m => m.EndTime, _ => dto.EndTime.AddMinutes(30))
            .Generate();

        _maintenance
            .Setup(m => m.GetByFacilityAsync(dto.FacilityId))
            .ReturnsAsync(new[] { maintenance });

        await ShouldThrowConflict(() => _validator.ValidateAsync((dto, 10)));
    }

    // ------------------------------------------------------------
    // 6. SLOT UNAVAILABLE
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenSlotUnavailable()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        _maintenance
            .Setup(m => m.GetByFacilityAsync(dto.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        _reservation
            .Setup(r => r.IsSlotAvailableAsync(dto.StartTime, dto.EndTime, dto.FacilityId))
            .ReturnsAsync(false);

        await ShouldThrowConflict(() => _validator.ValidateAsync((dto, 10)));
    }

    // ------------------------------------------------------------
    // 7. INVALID USER ID
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldThrow_WhenUserIdInvalid()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        _maintenance
            .Setup(m => m.GetByFacilityAsync(dto.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        _reservation
            .Setup(r => r.IsSlotAvailableAsync(dto.StartTime, dto.EndTime, dto.FacilityId))
            .ReturnsAsync(true);

        await ShouldThrowValidation(() => _validator.ValidateAsync((dto, 0)));
    }

    // ------------------------------------------------------------
    // 8. FULL VALID CASE
    // ------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldPass_WhenAllValid()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facility
            .Setup(f => f.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeData.Facility().Generate());

        _maintenance
            .Setup(m => m.GetByFacilityAsync(dto.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        _reservation
            .Setup(r => r.IsSlotAvailableAsync(dto.StartTime, dto.EndTime, dto.FacilityId))
            .ReturnsAsync(true);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync((dto, 10)));
    }
}
