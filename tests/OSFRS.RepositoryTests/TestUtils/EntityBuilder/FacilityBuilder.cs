using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.EntityBuilders;

public static class FacilityBuilder
{
    private static int _nextId = 1;

    public static Facility Create(
        int? id = null,
        string? name = null,
        string type = "Court",
        int capacity = 10,
        string status = "Available"
    )
    {
        int finalId = id ?? _nextId++;

        return new Facility
        {
            Id = finalId,
            Name = name ?? $"Facility {finalId}",
            Type = type,
            Capacity = capacity,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
