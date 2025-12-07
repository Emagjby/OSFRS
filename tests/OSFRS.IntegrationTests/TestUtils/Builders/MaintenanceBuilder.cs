using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Builders;

public class MaintenanceBuilder
{
    private int _facilityId = 1;
    private string _desc = "Some maintenance";
    private DateTime _start = DateTime.UtcNow.AddHours(1);
    private DateTime _end = DateTime.UtcNow.AddHours(2);
    private string _status = "Scheduled";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public static MaintenanceBuilder Create() => new();

    public MaintenanceBuilder WithFacility(int id)
    {
        _facilityId = id;
        return this;
    }

    public MaintenanceBuilder WithDescription(string desc)
    {
        _desc = desc;
        return this;
    }

    public MaintenanceBuilder WithStart(DateTime start)
    {
        _start = start;
        return this;
    }

    public MaintenanceBuilder WithEnd(DateTime end)
    {
        _end = end;
        return this;
    }

    public MaintenanceBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public MaintenanceRecord Build() =>
        new MaintenanceRecord
        {
            FacilityId = _facilityId,
            Description = _desc,
            StartTime = _start,
            EndTime = _end,
            Status = _status,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
        };
}
