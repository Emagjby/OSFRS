using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Validators;

public static class UsageEventValidator
{
    public static bool Validate(UsageEventDto dto)
    {
        if (dto == null)
            return false;

        if (string.IsNullOrEmpty(dto.EventType))
            return false;

        if (dto.EventType.Length > 50)
            return false;

        if (dto.Timestamp == default)
            return false;

        if (dto.UserId.HasValue && dto.UserId.Value <= 0)
            return false;

        if (dto.FacilityId.HasValue && dto.FacilityId.Value <= 0)
            return false;

        if (dto.Metadata != null)
        {
            foreach (var kv in dto.Metadata)
            {
                if (string.IsNullOrWhiteSpace(kv.Key))
                    return false;

                if (kv.Key.Length > 50)
                    return false;

                if (kv.Value == null)
                    return false;

                if (kv.Value.Length > 200)
                    return false;
            }
        }

        return true;
    }
}