using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.EntityBuilders;

public static class ReservationBuilder
{
    private static int _nextId = 1;

    public static Reservation Create(
        int? id = null,
        int userId = 1,
        int facilityId = 1,
        DateTime? start = null,
        DateTime? end = null,
        string status = "Pending"
    )
    {
        int finalId = id ?? _nextId++;

        var startTime = start ?? DateTime.UtcNow.AddHours(1);
        var endTime = end ?? startTime.AddHours(1);

        return new Reservation
        {
            Id = finalId,
            UserId = userId,
            FacilityId = facilityId,
            StartTime = startTime,
            EndTime = endTime,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
