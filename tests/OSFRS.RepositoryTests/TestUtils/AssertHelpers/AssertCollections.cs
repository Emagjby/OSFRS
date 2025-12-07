using FluentAssertions;
using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.AssertHelpers;

public static class AssertCollections
{
    public static void EqualUsers(IEnumerable<User> actual, IEnumerable<User> expected)
    {
        actual.Should().HaveSameCount(expected);
        actual.Zip(expected).ToList().ForEach(pair => AssertUser.Equal(pair.First, pair.Second));
    }

    public static void EqualFacilities(IEnumerable<Facility> actual, IEnumerable<Facility> expected)
    {
        actual.Should().HaveSameCount(expected);
        actual
            .Zip(expected)
            .ToList()
            .ForEach(pair => AssertFacility.Equal(pair.First, pair.Second));
    }

    public static void EqualMaintenance(
        IEnumerable<MaintenanceRecord> actual,
        IEnumerable<MaintenanceRecord> expected
    )
    {
        actual.Should().HaveSameCount(expected);
        actual
            .Zip(expected)
            .ToList()
            .ForEach(pair => AssertMaintenanceRecord.Equal(pair.First, pair.Second));
    }

    public static void EqualReservations(
        IEnumerable<Reservation> actual,
        IEnumerable<Reservation> expected
    )
    {
        actual.Should().HaveSameCount(expected);
        actual
            .Zip(expected)
            .ToList()
            .ForEach(pair => AssertReservation.Equal(pair.First, pair.Second));
    }

    public static void EqualUsageRecords(
        IEnumerable<UsageRecord> actual,
        IEnumerable<UsageRecord> expected
    )
    {
        actual.Should().HaveSameCount(expected);
        actual
            .Zip(expected)
            .ToList()
            .ForEach(pair => AssertUsageRecord.Equal(pair.First, pair.Second));
    }
}
