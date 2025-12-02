using OSFRS.Models.Entities;

namespace OSFRS.RepositoryTests.TestUtils.EntityBuilders;

public static class UserBuilder
{
    private static int _nextId = 1;

    public static User Create(
        int? id = null,
        string? username = null,
        string? email = null,
        string? role = "User",
        string? passwordHash = "hashed",
        string? name = "Test User"
    )
    {
        int finalId = id ?? _nextId++;

        return new User
        {
            Id = finalId,
            Username = username ?? $"user{finalId}",
            Email = email ?? $"user{finalId}@test.com",
            Role = role!,
            PasswordHash = passwordHash!,
            Name = name!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
