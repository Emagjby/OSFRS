using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;

namespace OSFRS.Backend.Validators.Usage;

/// <summary>
/// Validates query parameters used for filtering usage analytics and usage event lookups.
/// Ensures valid numeric identifiers, valid event types, and correct date ranges.
/// </summary>
public class UsageQueryValidator : BaseValidator, IValidator<(string? eventType, int? userId, int? facilityId, DateTime? from, DateTime? to)>
{
    /// <summary>
    /// Validates a usage query request.
    /// Checks ID validity, event type correctness, and time-range ordering.
    /// </summary>
    /// <param name="data">Data takes:
    /// the event type being queried (optional);
    /// the user ID filter (optional);
    /// the facility ID filter (optional);
    /// the start date/time (optional);
    /// and the end date/time (optional).</param>
    public Task ValidateAsync(
        (string? eventType,
        int? userId,
        int? facilityId,
        DateTime? from,
        DateTime? to) data)
    {
        (string? eventType,
        int? userId,
        int? facilityId,
        DateTime? from,
        DateTime? to) = data;

        if (userId.HasValue)
            Require(userId > 0, "UserId must be greater than zero.");

        if (facilityId.HasValue)
            Require(facilityId > 0, "FacilityId must be greater than zero.");

        if (eventType is not null)
            Require(
                UsageEventTypes.All.Contains(eventType),
                "Invalid event type."
            );

        if (from.HasValue && to.HasValue)
            Require(from <= to, "'from' must be <= 'to'.");

        return Task.CompletedTask;
    }
}