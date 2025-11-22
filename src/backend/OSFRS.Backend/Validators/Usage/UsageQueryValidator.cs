using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Validators.Base;

namespace OSFRS.Backend.Validators.Usage;

/// <summary>
/// Validates query parameters used for filtering usage analytics and usage event lookups.
/// Ensures valid numeric identifiers, valid event types, and correct date ranges.
/// </summary>
public class UsageQueryValidator : BaseValidator
{
    /// <summary>
    /// Validates a usage query request.
    /// Checks ID validity, event type correctness, and time-range ordering.
    /// </summary>
    /// <param name="eventType">Event type being queried (optional).</param>
    /// <param name="userId">User ID filter (optional).</param>
    /// <param name="facilityId">Facility ID filter (optional).</param>
    /// <param name="from">Start date/time (optional).</param>
    /// <param name="to">End date/time (optional).</param>
    public void Validate(
        string? eventType,
        int? userId,
        int? facilityId,
        DateTime? from,
        DateTime? to)
    {
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
    }
}