using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Builders;

public class ReservationBuilder
{
    private int _userId = 1;
    private int _facilityId = 1;
    private DateTime _start = DateTime.UtcNow.AddHours(1);
    private DateTime _end = DateTime.UtcNow.AddHours(2);
    private string _status = "Pending";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public static ReservationBuilder Create() => new();

    public ReservationBuilder WithUser(int userId)
    {
        _userId = userId;
        return this;
    }

    public ReservationBuilder WithFacility(int facilityId)
    {
        _facilityId = facilityId;
        return this;
    }

    public ReservationBuilder WithStart(DateTime start)
    {
        _start = start;
        return this;
    }

    public ReservationBuilder WithEnd(DateTime end)
    {
        _end = end;
        return this;
    }

    public ReservationBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public Reservation Build() =>
        new Reservation
        {
            UserId = _userId,
            FacilityId = _facilityId,
            StartTime = _start,
            EndTime = _end,
            Status = _status,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
        };
}
