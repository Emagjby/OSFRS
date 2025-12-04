using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Builders;

public class FacilityBuilder
{
    private string _name = "Test Facility";
    private string _type = "Gym";
    private int _capacity = 10;
    private string _status = "Available";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public static FacilityBuilder Create() => new();

    public FacilityBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public FacilityBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public FacilityBuilder WithCapacity(int capacity)
    {
        _capacity = capacity;
        return this;
    }

    public FacilityBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public Facility Build() =>
        new Facility
        {
            Name = _name,
            Type = _type,
            Capacity = _capacity,
            Status = _status,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
        };
}
