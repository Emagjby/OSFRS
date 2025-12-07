using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils;

public static class TestData
{
    public static User User(int id = 1) =>
        new()
        {
            Id = id,
            Username = $"user{id}",
            Email = $"user{id}@mail.com",
            Name = "Test User",
            PasswordHash = "HASH",
        };

    public static Facility Facility(int id = 1) =>
        new()
        {
            Id = id,
            Name = $"Facility{id}",
            Type = "Court",
            Capacity = 10,
            Status = "Available",
        };
}
