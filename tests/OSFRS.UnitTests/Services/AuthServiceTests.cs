using FluentAssertions;
using Moq;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repo;
    private readonly Mock<IPasswordHasher> _hasher;
    private readonly Mock<IJwtTokenGenerator> _jwt;
    private readonly Mock<IAppLogger<AuthService>> _logger;

    private readonly Mock<IValidator<LoginRequestDto>> _loginValidator;
    private readonly Mock<IValidator<UserRegistrationDto>> _registrationValidator;

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _repo = MockFactories.UserRepo();
        _hasher = MockFactories.Hasher();
        _jwt = MockFactories.Jwt();
        _logger = MockFactories.Logger<AuthService>();

        _loginValidator = MockFactories.Validator<LoginRequestDto>();
        _registrationValidator = MockFactories.Validator<UserRegistrationDto>();

        _service = new AuthService(
            _repo.Object,
            _hasher.Object,
            _jwt.Object,
            _logger.Object,
            _loginValidator.Object,
            _registrationValidator.Object
        );
    }

    // ============================================================
    // LOGIN
    // ============================================================

    [Fact]
    public async Task Login_ShouldCallValidator()
    {
        var dto = FakeData.LoginRequest().Generate();

        var user = FakeData.User().Generate();

        _repo.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail)).ReturnsAsync(user);

        _hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        int? EXPIRY = null;
        _jwt.Setup(j => j.GenerateToken(user, EXPIRY)).Returns("fake-token");

        await _service.LoginAsync(dto);

        _loginValidator.Verify(v => v.ValidateAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenUserNotFound()
    {
        var dto = FakeData.LoginRequest().Generate();

        _repo
            .Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail))
            .ReturnsAsync((User?)null);

        var act = async () => await _service.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordWrong()
    {
        var dto = FakeData.LoginRequest().Generate();

        var user = FakeData.User().Generate();
        _repo.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail)).ReturnsAsync(user);

        _hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

        var act = async () => await _service.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsValid()
    {
        var dto = FakeData.LoginRequest().Generate();

        var user = FakeData.User().Generate();
        _repo.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail)).ReturnsAsync(user);

        _hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        var TOKEN = "jwt-token-example";
        int? EXPIRY = null;
        _jwt.Setup(j => j.GenerateToken(user, EXPIRY)).Returns(TOKEN);

        var result = await _service.LoginAsync(dto);

        result.Should().Be(TOKEN);
        _jwt.Verify(j => j.GenerateToken(user, EXPIRY), Times.Once);
    }

    // ============================================================
    // REGISTRATION
    // ============================================================

    [Fact]
    public async Task Register_ShouldCallRegistrationValidator()
    {
        var dto = FakeData.RegistrationDto().Generate();

        await _service.RegisterUserAsync(dto);

        _registrationValidator.Verify(v => v.ValidateAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Register_ShouldHashPassword_AndSaveUser()
    {
        var dto = FakeData.RegistrationDto().Generate();

        var HASHED = "hashed-password";
        _hasher.Setup(h => h.Hash(dto.Password)).Returns(HASHED);

        await _service.RegisterUserAsync(dto);

        _repo.Verify(
            r =>
                r.AddAsync(
                    It.Is<User>(u => u.PasswordHash == HASHED),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
