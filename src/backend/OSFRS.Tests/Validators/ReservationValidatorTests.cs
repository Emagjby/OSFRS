using Xunit;
using OSFRS.Backend.Validators;
using OSFRS.Backend.DTOs;

namespace OSFRS.Tests.Validators;

public class ReservationValidatorTests
{

    [Fact]
    public void ValidateTimes_ReturnsTrue_ForValidRange()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);

        var result = ReservationValidator.ValidateTimes(start, end);

        Assert.True(result);
    }

    [Fact]
    public void ValidateTimes_ReturnsFalse_WhenEndBeforeStart()
    {
        var start = DateTime.UtcNow.AddHours(3);
        var end = start.AddHours(-1);

        var result = ReservationValidator.ValidateTimes(start, end);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTimes_ReturnsFalse_WhenStartInPast()
    {
        var start = DateTime.UtcNow.AddHours(-1);
        var end = DateTime.UtcNow.AddHours(1);

        var result = ReservationValidator.ValidateTimes(start, end);

        Assert.False(result);
    }

    [Fact]
    public void ValidateFacilityId_ReturnsFalse_WhenZeroOrNegative()
    {
        Assert.False(ReservationValidator.ValidateFacilityId(0));
        Assert.False(ReservationValidator.ValidateFacilityId(-1));
    }

    [Fact]
    public void ValidateUserId_ReturnsFalse_WhenZeroOrNegative()
    {
        Assert.False(ReservationValidator.ValidateUserId(0));
        Assert.False(ReservationValidator.ValidateUserId(-10));
    }

    [Fact]
    public void ValidateReservationId_ReturnsFalse_WhenZeroOrNegative()
    {
        Assert.False(ReservationValidator.ValidateReservationId(0));
        Assert.False(ReservationValidator.ValidateReservationId(-5));
    }

    [Fact]
    public void ValidateUpdate_ReturnsTrue_ForValidChanges()
    {
        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(5), // Changed end time (was 4, now 5)
            Status = "Pending"
        };

        var result = ReservationValidator.ValidateUpdate(updateDto);

        Assert.True(result);
    }

    [Fact]
    public void ValidateUpdate_ReturnsFalse_WhenNoChanges()
    {
        var updateDto = new UpdateReservationDto
        {
            StartTime = default,
            EndTime = default,
            Status = default
        };

        var result = ReservationValidator.ValidateUpdate(updateDto);

        Assert.False(result);
    }

    [Fact]
    public void ValidateUpdate_ReturnsFalse_WhenEndBeforeStart()
    {
        var updateDto = new UpdateReservationDto
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1), // End before start
            Status = "Pending"
        };

        var result = ReservationValidator.ValidateUpdate(updateDto);

        Assert.False(result);
    }
}