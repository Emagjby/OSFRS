using FluentAssertions;
using OSFRS.Backend.Exceptions;
using OSFRS.Models.Entities;

namespace OSFRS.UnitTests.TestUtils;

public static class ValidatorTestHelpers
{
    public static async Task ShouldThrowValidation(Func<Task> act)
    {
        await act.Should().ThrowAsync<ValidationException>();
    }

    public static async Task ShouldNotThrowValidation(Func<Task> act)
    {
        await act.Should().NotThrowAsync();
    }

    public static async Task ShouldThrowConflict(Func<Task> act)
    {
        await act.Should().ThrowAsync<ConflictException>();
    }

    public static async Task ShouldThrowNotFound(Func<Task> act)
    {
        await act.Should().ThrowAsync<NotFoundException>();
    }

    public static async Task ShouldThrowPastDate(Func<Task> act)
    {
        await act.Should().ThrowAsync<PastDateException>();
    }

    public static User ExistingUser =>
        new User
        {
            Id = 10,
            Name = "Existing User",
            Username = "old_username",
            Email = "old@mail.com",
        };

    public static Reservation Existing(int uid = 10) =>
        FakeData
            .Reservation()
            .RuleFor(r => r.UserId, _ => uid)
            .RuleFor(r => r.StartTime, _ => DateTime.UtcNow.AddHours(2))
            .RuleFor(r => r.EndTime, _ => DateTime.UtcNow.AddHours(4))
            .RuleFor(r => r.Status, _ => "Pending")
            .Generate();

    public static Reservation OwnedReservation(int userId = 10) =>
        FakeData
            .Reservation()
            .RuleFor(r => r.UserId, _ => userId)
            .RuleFor(r => r.Status, _ => "Pending")
            .Generate();

    public static Facility ExistingFacility = new()
    {
        Id = 10,
        Name = "Hall A",
        Type = "Gym",
        Capacity = 20,
        Status = "Available",
    };

    public static Facility ExistingCourt = new Facility
    {
        Id = 1,
        Name = "Court A",
        Type = "Gym",
        Capacity = 10,
        Status = "Unavailable",
    };
}
