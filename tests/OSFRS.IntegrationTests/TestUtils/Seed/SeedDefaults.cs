using OSFRS.IntegrationTests.TestUtils.Builders;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Seed;

public static class SeedDefaults
{
    /// <summary>
    /// Creates a default user (not saved).
    /// </summary>
    public static User DefaultUser() =>
        UserBuilder
            .Create()
            .WithName("Default User")
            .WithUsername("default_user")
            .WithEmail("default@mail.com")
            .Build();

    /// <summary>
    /// Creates a default facility (not saved).
    /// </summary>
    public static Facility DefaultFacility() =>
        FacilityBuilder
            .Create()
            .WithName("Default Facility")
            .WithType("Gym")
            .WithCapacity(20)
            .Build();

    /// <summary>
    /// Creates a default reservation (not saved).
    /// </summary>
    public static Reservation DefaultReservation(int userId, int facilityId) =>
        ReservationBuilder
            .Create()
            .WithUser(userId)
            .WithFacility(facilityId)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(2))
            .Build();

    /// <summary>
    /// Creates a default maintenance record (not saved).
    /// </summary>
    public static MaintenanceRecord DefaultMaintenance(int facilityId) =>
        MaintenanceBuilder
            .Create()
            .WithFacility(facilityId)
            .WithStart(DateTime.UtcNow.AddHours(1))
            .WithEnd(DateTime.UtcNow.AddHours(3))
            .Build();

    /// <summary>
    /// Creates a default usage record (not saved).
    /// </summary>
    public static UsageRecord DefaultUsage(int? userId = null, int? facilityId = null) =>
        UsageBuilder.Create().ForUser(userId).ForFacility(facilityId).At(DateTime.UtcNow).Build();
}
