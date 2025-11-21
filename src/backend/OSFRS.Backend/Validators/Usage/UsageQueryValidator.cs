using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Validators.Base;

namespace OSFRS.Backend.Validators.Usage;

public class UsageQueryValidator : BaseValidator
{
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
            Require(UsageEventTypes.All.Contains(eventType),
                "Invalid event type.");

        if (from.HasValue && to.HasValue)
            Require(from <= to, "'from' must be <= 'to'.");
    }
}