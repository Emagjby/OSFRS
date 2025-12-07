using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.TestUtils.Builders;

public class UserBuilder
{
    private string _name = "John Doe";
    private string _username = $"user_{Guid.NewGuid():N}".Substring(0, 10);
    private string _email = $"user_{Guid.NewGuid():N}@mail.com";
    private string _passwordHash = "HASH";
    private string _role = "User";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public static UserBuilder Create() => new();

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPassword(string hash)
    {
        _passwordHash = hash;
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _role = "Admin";
        return this;
    }

    public User Build() =>
        new User
        {
            Name = _name,
            Username = _username,
            Email = _email,
            PasswordHash = _passwordHash,
            Role = _role,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
        };
}
