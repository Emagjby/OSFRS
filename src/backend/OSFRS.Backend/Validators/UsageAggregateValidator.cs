using OSFRS.Backend.DTOs.Analytics;

namespace OSFRS.Backend.Validators;

public static class UsageAggregateValidator
{
    public static bool Validate(UsageAggregateDto dto)
    {
        if (dto == null)
            return false;

        if (string.IsNullOrWhiteSpace(dto.EventType))
            return false;

        if (dto.EventType.Length > 50)
            return false;

        if (dto.Count < 0)
            return false;

        if (dto.PeriodStart == default || dto.PeriodEnd == default)
            return false;

        if (dto.PeriodEnd < dto.PeriodStart)
            return false;

        if (dto.UserId.HasValue && dto.UserId.Value <= 0)
            return false;

        if (dto.FacilityId.HasValue && dto.FacilityId.Value <= 0)
            return false;

        return true;
    }
}