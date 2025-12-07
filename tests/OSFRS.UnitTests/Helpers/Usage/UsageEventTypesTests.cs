using FluentAssertions;
using OSFRS.Backend.Helpers.Usage;

namespace OSFRS.UnitTests.Helpers.Usage;

public class UsageEventTypesTests
{
    [Fact]
    public void All_ShouldContainEveryDefinedConstant()
    {
        // Reflect all public const string fields
        var constFields = typeof(UsageEventTypes)
            .GetFields(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.FlattenHierarchy
            )
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => f.GetValue(null)!.ToString())
            .ToList();

        UsageEventTypes.All.Should().BeEquivalentTo(constFields);
    }

    [Fact]
    public void All_ShouldHaveNoDuplicates()
    {
        UsageEventTypes
            .All.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Should()
            .BeEmpty("event types must be unique");
    }

    [Fact]
    public void Constants_ShouldHaveCorrectStringValues()
    {
        UsageEventTypes.ReservationCreated.Should().Be("ReservationCreated");
        UsageEventTypes.ReservationUpdated.Should().Be("ReservationUpdated");
        UsageEventTypes.ReservationCancelled.Should().Be("ReservationCancelled");
        UsageEventTypes.ReservationDeleted.Should().Be("ReservationDeleted");
        UsageEventTypes.ReservationAdminUpdated.Should().Be("ReservationAdminUpdated");

        UsageEventTypes.FacilityCreated.Should().Be("FacilityCreated");
        UsageEventTypes.FacilityUpdated.Should().Be("FacilityUpdated");
        UsageEventTypes.FacilityDeleted.Should().Be("FacilityDeleted");
        UsageEventTypes.FacilityAvailabilityChanged.Should().Be("FacilityAvailabilityChanged");

        UsageEventTypes.MaintenanceScheduled.Should().Be("MaintenanceScheduled");
        UsageEventTypes.MaintenanceUpdated.Should().Be("MaintenanceUpdated");
        UsageEventTypes.MaintenanceDeleted.Should().Be("MaintenanceDeleted");

        UsageEventTypes.StatusSyncRun.Should().Be("StatusSyncRun");
        UsageEventTypes.AggregateComputed.Should().Be("AggregateComputed");
    }
}
