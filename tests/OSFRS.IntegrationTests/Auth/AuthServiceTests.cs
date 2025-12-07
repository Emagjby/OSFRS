using FluentAssertions;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Auth;

public class AuthService_IntegrationTests : IntegrationTestBase
{
    public AuthService_IntegrationTests()
        : base("OSFRS_IT_AuthService") { }

    // -------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------
    private IAuthService CreateService() => Factory.AuthService();

    private IUserRepository UserRepo() => Factory.UserRepo();

    private IPasswordHasher Hasher() => Factory.PasswordHasher();

    private IJwtTokenGenerator Jwt() => Factory.Jwt();

    // =============================================================
    // REGISTRATION
    // =============================================================

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUser_WhenDataIsValid()
    {
        var service = CreateService();

        var dto = new UserRegistrationDto
        {
            Name = "Gencho",
            Username = "gencho123",
            Email = "gencho@mail.com",
            Password = "StrongPass123!",
        };

        await service.RegisterUserAsync(dto);

        var user = await UserRepo().GetByUsernameOrEmailAsync("gencho123");
        user.Should().NotBeNull();
        user!.Email.Should().Be(dto.Email);

        Hasher().Verify(dto.Password, user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        var service = CreateService();

        await UserRepo().AddAsync(UserBuilder.Create().WithEmail("x@mail.com").Build());
        await UserRepo().SaveChangesAsync();

        var dto = new UserRegistrationDto
        {
            Name = "Test",
            Username = "newuser",
            Email = "x@mail.com",
            Password = "Pass123!",
        };

        var act = async () => await service.RegisterUserAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldFail_WhenUsernameAlreadyExists()
    {
        var service = CreateService();

        await UserRepo().AddAsync(UserBuilder.Create().WithUsername("taken").Build());
        await UserRepo().SaveChangesAsync();

        var dto = new UserRegistrationDto
        {
            Name = "Test",
            Username = "taken",
            Email = "unique@mail.com",
            Password = "Pass123!",
        };

        var act = async () => await service.RegisterUserAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("ThisNameIsWayTooLongToBeValidBecauseItExceeds50Characters")]
    [InlineData("Gencho123")]
    [InlineData("Ivan__Ivanov")]
    [InlineData("Ivan  Ivanov")]
    public async Task RegisterUserAsync_ShouldFail_WhenNameInvalid(string badName)
    {
        var service = CreateService();

        var dto = new UserRegistrationDto
        {
            Name = badName,
            Username = "validusername",
            Email = "valid@mail.com",
            Password = "StrongPass123!",
        };

        var act = async () => await service.RegisterUserAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldFail_WhenPasswordWeak()
    {
        var service = CreateService();

        var dto = new UserRegistrationDto
        {
            Name = "Valid Name",
            Username = "validusername",
            Email = "valid@mail.com",
            Password = "weak",
        };

        var act = async () => await service.RegisterUserAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // =============================================================
    // LOGIN
    // =============================================================

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenUsernameAndPasswordValid()
    {
        var service = CreateService();

        var user = UserBuilder
            .Create()
            .WithUsername("gencho")
            .WithPassword(Hasher().Hash("StrongPass123!"))
            .Build();
        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var dto = new LoginRequestDto { UsernameOrEmail = "gencho", Password = "StrongPass123!" };

        var token = await service.LoginAsync(dto);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenEmailAndPasswordValid()
    {
        var service = CreateService();

        var user = UserBuilder
            .Create()
            .WithEmail("x@mail.com")
            .WithPassword(Hasher().Hash("Pass123!"))
            .Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var dto = new LoginRequestDto { UsernameOrEmail = "x@mail.com", Password = "Pass123!" };

        var token = await service.LoginAsync(dto);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIncorrect()
    {
        var service = CreateService();

        var user = UserBuilder
            .Create()
            .WithUsername("gencho")
            .WithPassword(Hasher().Hash("Correct123!"))
            .Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var dto = new LoginRequestDto { UsernameOrEmail = "gencho", Password = "WrongPass" };

        var act = async () => await service.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        var service = CreateService();

        var dto = new LoginRequestDto { UsernameOrEmail = "missing", Password = "anything" };

        var act = async () => await service.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("", "validpass")] // empty username/email
    [InlineData("invalid email@", "pass123")] // invalid email
    [InlineData("bad user!", "pass123")] // invalid username format
    [InlineData("user123", "")] // missing password
    public async Task LoginAsync_ShouldThrow_WhenValidatorFails(string id, string pw)
    {
        var service = CreateService();

        var dto = new LoginRequestDto { UsernameOrEmail = id, Password = pw };

        var act = async () => await service.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnDifferentTokens_OnMultipleCalls()
    {
        var service = CreateService();

        var user = UserBuilder
            .Create()
            .WithUsername("uniqueuser")
            .WithPassword(Hasher().Hash("Strong123!"))
            .Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var dto = new LoginRequestDto { UsernameOrEmail = "uniqueuser", Password = "Strong123!" };

        var a = await service.LoginAsync(dto);
        var b = await service.LoginAsync(dto);

        a.Should().NotBe(b);
    }
}
