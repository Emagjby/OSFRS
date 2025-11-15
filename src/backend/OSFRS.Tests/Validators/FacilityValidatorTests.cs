using OSFRS.Backend.DTOs;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Tests.Validators;

public class FacilityValidatorTests
{
    [Fact]
    public void ValidateCreate_ValidSportsFacility_ReturnsTrue()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Basketball Court",
            Type = "Indoor",
            Capacity = 50,
            Status = "Available"
        };

        var result = FacilityValidator.ValidateCreate(dto, out string error);

        Assert.True(result);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void ValidateCreate_EmptyName_ReturnsError()
    {
        var dto = new CreateFacilityDto
        {
            Name = "",
            Type = "Gym",
            Capacity = 20,
            Status = "Available"
        };

        var result = FacilityValidator.ValidateCreate(dto, out string error);

        Assert.False(result);
        Assert.Equal("Facility name cannot be empty.", error);
    }

    [Fact]
    public void ValidateCreate_NullType_ReturnsError()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Swimming Pool",
            Type = null!,
            Capacity = 30,
            Status = "Available"
        };

        var result = FacilityValidator.ValidateCreate(dto, out string error);

        Assert.False(result);
        Assert.Equal("Facility type cannot be empty.", error);
    }

    [Fact]
    public void ValidateCreate_CapacityLessThanOne_ReturnsError()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Tennis Court",
            Type = "Outdoor",
            Capacity = 0,
            Status = "Available"
        };

        var result = FacilityValidator.ValidateCreate(dto, out string error);

        Assert.False(result);
        Assert.Equal("Facility capacity must be greater than zero.", error);
    }

    [Fact]
    public void ValidateCreate_CapacityGreaterThan1000_ReturnsError()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Mega Arena",
            Type = "Indoor",
            Capacity = 1500,
            Status = "Available"
        };

        var result = FacilityValidator.ValidateCreate(dto, out string error);

        Assert.False(result);
        Assert.Equal("Facility capacity must be less than a thousand.", error);
    }

    [Fact]
    public void ValidateCreate_EmptyStatus_ReturnsError()
    {
        var dto = new CreateFacilityDto
        {
            Name = "Football Field",
            Type = "Outdoor",
            Capacity = 100,
            Status = ""
        };

        var result = FacilityValidator.ValidateCreate(dto, out string error);

        Assert.False(result);
        Assert.Equal("Facility status cannot be empty.", error);
    }

    [Fact]
    public void ValidateUpdate_NullDto_ReturnsError()
    {
        Facility existing = new Facility { Id = 1, Name = "Old Field" };

        var result = FacilityValidator.ValidateUpdate(null!, existing, out string error);

        Assert.False(result);
        Assert.Equal("Update data cannot be null.", error);
    }

    [Fact]
    public void ValidateUpdate_NoFieldsProvided_ReturnsError()
    {
        var dto = new UpdateFacilityDto();
        var existing = new Facility { Id = 1, Name = "Training Ground" };

        var result = FacilityValidator.ValidateUpdate(dto, existing, out string error);

        Assert.False(result);
        Assert.Equal("No updates provided.", error);
    }

    [Fact]
    public void ValidateUpdate_CapacityLessThanOne_ReturnsError()
    {
        var dto = new UpdateFacilityDto { Capacity = 0 };
        var existing = new Facility { Id = 1, Name = "Court A" };

        var result = FacilityValidator.ValidateUpdate(dto, existing, out string error);

        Assert.False(result);
        Assert.Equal("Facility capacity must be greater than zero.", error);
    }

    [Fact]
    public void ValidateUpdate_CapacityGreaterThan1000_ReturnsError()
    {
        var dto = new UpdateFacilityDto { Capacity = 2000 };
        var existing = new Facility { Id = 1, Name = "Pool" };

        var result = FacilityValidator.ValidateUpdate(dto, existing, out string error);

        Assert.False(result);
        Assert.Equal("Facility capacity must be less than a thousand.", error);
    }

    [Fact]
    public void ValidateUpdate_ValidNameOnly_ReturnsTrue()
    {
        var dto = new UpdateFacilityDto { Name = "New Field Name" };
        var existing = new Facility { Id = 1, Name = "Old Field" };

        var result = FacilityValidator.ValidateUpdate(dto, existing, out string error);

        Assert.True(result);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void ValidateUpdate_WhitespaceNameAndValidCapacity_ReturnsTrue()
    {
        var dto = new UpdateFacilityDto { Name = "   ", Capacity = 20 };
        var existing = new Facility { Id = 1, Name = "Court C" };

        var result = FacilityValidator.ValidateUpdate(dto, existing, out string error);

        Assert.True(result);
        Assert.Equal(string.Empty, error);
    }
}