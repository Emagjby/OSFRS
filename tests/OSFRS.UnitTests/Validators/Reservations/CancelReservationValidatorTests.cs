using OSFRS.Backend.Validators.Reservations;
using OSFRS.Models.Entities;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class CancelReservationValidatorTests
{
    private readonly CancelReservationValidator _validator;

    public CancelReservationValidatorTests()
    {
        _validator = new CancelReservationValidator();
    }

    // ============================================================
    // NOT FOUND
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrowNotFound_WhenReservationIsNull()
    {
        Reservation? missing = null;

        await ShouldThrowNotFound(() => _validator.ValidateAsync((missing!, 10)));
    }

    // ============================================================
    // OWNERSHIP RULE
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenUserDoesNotOwnReservation()
    {
        var reservation = OwnedReservation(userId: 5);

        await ShouldThrowValidation(() => _validator.ValidateAsync((reservation, userId: 999)));
    }

    // ============================================================
    // ALREADY CANCELLED
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrowConflict_WhenAlreadyCancelled()
    {
        var reservation = OwnedReservation();
        reservation.Status = "Cancelled";

        await ShouldThrowConflict(() =>
            _validator.ValidateAsync((reservation, reservation.UserId))
        );
    }

    // ============================================================
    // VALID CASE
    // ============================================================

    [Fact]
    public async Task Validate_ShouldPass_WhenOwnedAndNotCancelled()
    {
        var reservation = OwnedReservation();
        reservation.Status = "Pending";

        await ShouldNotThrowValidation(() =>
            _validator.ValidateAsync((reservation, reservation.UserId))
        );
    }
}
