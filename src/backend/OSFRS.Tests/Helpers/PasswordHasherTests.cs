using OSFRS.Backend.Helpers;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Tests.Helpers;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnNonNullDifferentString()
    {
        string password = "Strongp@ssword123";
        string hash = _passwordHasher.Hash(password);

        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        string password = "Strongp@ssword123";
        string hash = _passwordHasher.Hash(password);

        bool result = _passwordHasher.Verify(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        string password = "Strongp@ssword123";
        string wrongPassword = "Wrongp@ssword456";
        string hash = _passwordHasher.Hash(password);

        bool result = _passwordHasher.Verify(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForSamePassword()
    {
        string password = "Strongp@ssword123";
        string hash1 = _passwordHasher.Hash(password);
        string hash2 = _passwordHasher.Hash(password);

        Assert.NotEqual(hash1, hash2);
    }
}