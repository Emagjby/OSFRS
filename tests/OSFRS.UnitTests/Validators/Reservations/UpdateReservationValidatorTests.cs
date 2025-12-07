using Moq;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Reservations;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class UpdateReservationValidatorTests
{
    private readonly Mock<IMaintenanceRepository> _maintenance;
    private readonly Mock<IReservationRepository> _reservation;

    private readonly UpdateReservationValidator _validator;

    public UpdateReservationValidatorTests()
    {
        _maintenance = MockFactories.MaintenanceRepo();
        _reservation = MockFactories.ReservationRepo();

        _validator = new UpdateReservationValidator(_maintenance.Object, _reservation.Object);
    }

    // ============================================================
    // OWNERSHIP RULES
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNonAdminModifiesForeignReservation()
    {
        var dto = new UpdateReservationDto();
        var existing = Existing(uid: 5);

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: 100))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAdminModifiesForeignReservation()
    {
        var dto = new UpdateReservationDto();
        var existing = Existing(uid: 5);

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 100))
        );
    }

    // ============================================================
    // CANCELLED RESERVATIONS
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNonAdminModifiesCancelledReservation()
    {
        var dto = new UpdateReservationDto();
        var existing = Existing();
        existing.Status = "Cancelled";

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAdminModifiesCancelledReservation()
    {
        var dto = new UpdateReservationDto();
        var existing = Existing();
        existing.Status = "Cancelled";

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 999))
        );
    }

    // ============================================================
    // PAST RESERVATIONS
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNonAdminModifiesPastReservation()
    {
        var dto = new UpdateReservationDto();

        var existing = Existing();
        existing.StartTime = DateTime.UtcNow.AddHours(-5);

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAdminModifiesPastReservation()
    {
        var dto = new UpdateReservationDto();

        var existing = Existing();
        existing.StartTime = DateTime.UtcNow.AddHours(-5);

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 100))
        );
    }

    // ============================================================
    // STATUS RULES
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNonAdminChangesStatus()
    {
        var dto = new UpdateReservationDto { Status = "Confirmed" };
        var existing = Existing();

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenStatusInvalid()
    {
        var dto = new UpdateReservationDto { Status = "SuperSaiyan" };
        var existing = Existing();

        await ShouldThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAdminSetsValidStatus()
    {
        var dto = new UpdateReservationDto { Status = "Confirmed" };
        var existing = Existing();

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 1))
        );
    }

    // ============================================================
    // TIME PAYLOAD RULES
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenOnlyStartProvided()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(10),
            EndTime = null,
        };

        var existing = Existing();

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenOnlyEndProvided()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = null,
            EndTime = DateTime.UtcNow.AddHours(10),
        };

        var existing = Existing();

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAdminOmitsTimes()
    {
        var dto = new UpdateReservationDto();
        var existing = Existing();

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 100))
        );
    }

    // ============================================================
    // TIME WINDOW VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNewStartAfterEnd()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(4),
        };

        var existing = Existing();

        await ShouldThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 1))
        );
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenStartInPast_ForNonAdmin()
    {
        var dto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(-3),
            EndTime = DateTime.UtcNow.AddHours(2),
        };

        var existing = Existing();

        await ShouldThrowPastDate(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    // ============================================================
    // MAINTENANCE CONFLICTS
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenOverlapsMaintenance()
    {
        var existing = Existing();
        var dto = FakeData
            .UpdateReservationDto()
            .RuleFor(r => r.StartTime, _ => DateTime.UtcNow.AddHours(2))
            .RuleFor(r => r.EndTime, _ => DateTime.UtcNow.AddHours(3))
            .Generate();

        var maintenance = FakeData
            .MaintenanceRecord()
            .RuleFor(m => m.StartTime, _ => existing.StartTime.AddMinutes(-30))
            .RuleFor(m => m.EndTime, _ => existing.EndTime.AddMinutes(30))
            .Generate();

        _maintenance
            .Setup(m => m.GetByFacilityAsync(existing.FacilityId))
            .ReturnsAsync(new[] { maintenance });

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    // ============================================================
    // RESERVATION CONFLICTS
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenOverlapsOtherReservation()
    {
        var existing = Existing();

        var dto = new UpdateReservationDto
        {
            StartTime = existing.StartTime.AddMinutes(5),
            EndTime = existing.EndTime.AddMinutes(5),
        };

        _maintenance
            .Setup(m => m.GetByFacilityAsync(existing.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        _reservation
            .Setup(r =>
                r.HasConflictAsync(
                    existing.FacilityId,
                    dto.StartTime!.Value,
                    dto.EndTime!.Value,
                    existing.Id
                )
            )
            .ReturnsAsync(true);

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    // ============================================================
    // VALID CASES
    // ============================================================

    [Fact]
    public async Task Validate_ShouldPass_WhenAllValid_ForUser()
    {
        var existing = Existing();
        var dto = new UpdateReservationDto
        {
            StartTime = existing.StartTime.AddHours(1),
            EndTime = existing.EndTime.AddHours(1),
        };

        _maintenance
            .Setup(m => m.GetByFacilityAsync(existing.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        _reservation
            .Setup(r =>
                r.HasConflictAsync(
                    existing.FacilityId,
                    dto.StartTime.Value,
                    dto.EndTime.Value,
                    existing.Id
                )
            )
            .ReturnsAsync(false);

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: false, userId: existing.UserId))
        );
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAllValid_ForAdmin()
    {
        var existing = Existing();
        var dto = new UpdateReservationDto();

        _maintenance
            .Setup(m => m.GetByFacilityAsync(existing.FacilityId))
            .ReturnsAsync(Array.Empty<MaintenanceRecord>());

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((dto, existing, isAdmin: true, userId: 999))
        );
    }
}
