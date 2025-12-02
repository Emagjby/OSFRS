using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.EntityBuilders;

public static class MaintenanceBuilder
{
    private static int _nextId = 1;

    public static MaintenanceRecord Create(
        int? id = null,
        int facilityId = 1,
        DateTime? start = null,
        DateTime? end = null,
        string status = "Scheduled"
    )
    {
        int finalId = id ?? _nextId++;

        var startTime = start ?? DateTime.UtcNow.AddHours(1);
        var endTime = end ?? startTime.AddHours(1);

        return new MaintenanceRecord
        {
            Id = finalId,
            FacilityId = facilityId,
            StartTime = startTime,
            EndTime = endTime,
            Status = status,
        };
    }
}
