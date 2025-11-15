namespace OSFRS.Backend.Helpers.Usage;

public class UsageEventTypes
{
    public const string ReservationCreated = "ReservationCreated";
    public const string ReservationUpdated = "ReservationUpdated";
    public const string ReservationCancelled = "ReservationCancelled";
    public const string ReservationDeleted = "ReservationDeleted";
    public const string ReservationAdminUpdated = "ReservationAdminUpdated";

    public const string FacilityCreated = "FacilityCreated";
    public const string FacilityUpdated = "FacilityUpdated";
    public const string FacilityDeleted = "FacilityDeleted";
    public const string FacilityAvailabilityChanged = "FacilityAvailabilityChanged";

    public const string MaintenanceScheduled = "MaintenanceScheduled";
    public const string MaintenanceUpdated = "MaintenanceUpdated";
    public const string MaintenanceDeleted = "MaintenanceDeleted";

    public const string StatusSyncRun = "StatusSyncRun";
    public const string AggregateComputed = "AggregateComputed";
}