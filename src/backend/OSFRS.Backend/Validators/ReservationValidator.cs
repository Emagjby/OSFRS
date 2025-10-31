using System;

namespace OSFRS.Backend.Validators;

public static class ReservationValidator
{
    public static bool ValidateTimes(DateTime start, DateTime end) =>
        end > start && start >= DateTime.UtcNow;

    public static bool ValidateFacilityId(int facilityId) =>
        facilityId > 0;

    public static bool ValidateUserId(int userId) =>
        userId > 0;
}