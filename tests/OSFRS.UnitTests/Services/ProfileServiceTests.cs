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

public class ProfileServiceTests
{
    private readonly Mock<IUserRepository> _repo;
    private readonly Mock<IPasswordHasher> _hasher;
    private readonly Mock<IAppLogger<ProfileService>> _logger;
    private readonly Mock<IUpdateValidator<UpdatedProfileDto, User>> _validator;

    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _repo = MockFactories.UserRepo();
        _hasher = MockFactories.Hasher();
        _logger = MockFactories.Logger<ProfileService>();
        _validator = MockFactories.UpdateValidator<UpdatedProfileDto, User>();

        _service = new ProfileService(
            _repo.Object,
            _hasher.Object,
            _logger.Object,
            _validator.Object
        );
    }

    // ============================================================
    // GET PROFILE
    // ============================================================

    [Fact]
    public async Task GetProfile_ShouldReturnMappedDto()
    {
        var user = FakeData.User().Generate();

        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _service.GetProfileAsync(user.Id);

        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetProfile_ShouldThrow_WhenUserMissing()
    {
        var MISSING = 999;

        _repo
            .Setup(r => r.GetByIdAsync(MISSING, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _service.GetProfileAsync(MISSING);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    [Fact]
    public async Task Update_ShouldThrowNotFound_WhenUserMissing()
    {
        var MISSING = 222;
        var dto = FakeData.UpdatedProfileDto().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(MISSING, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _service.UpdateProfileAsync(MISSING, dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ShouldCallValidator()
    {
        var user = FakeData.User().Generate();
        var dto = FakeData.UpdatedProfileDto().Generate();

        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _service.UpdateProfileAsync(user.Id, dto);

        _validator.Verify(v => v.ValidateAsync(dto, user), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldModifyOnlyProvidedFields()
    {
        var user = FakeData.User().Generate();

        user.Name = "OldName";
        user.Username = "OldUsername";
        user.Email = "old@mail.com";

        var dto = new UpdatedProfileDto
        {
            Name = "NewName",
            Email = "new@mail.com",
            Username = null, // stays same
        };

        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _service.UpdateProfileAsync(user.Id, dto);

        user.Name.Should().Be("NewName");
        user.Email.Should().Be("new@mail.com");
        user.Username.Should().Be("OldUsername");

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldUpdateUpdatedAt()
    {
        var user = FakeData.User().Generate();
        var dto = new UpdatedProfileDto { Name = "Hey" };

        var before = user.UpdatedAt;

        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _service.UpdateProfileAsync(user.Id, dto);

        user.UpdatedAt.Should().BeAfter(before);
    }
}
