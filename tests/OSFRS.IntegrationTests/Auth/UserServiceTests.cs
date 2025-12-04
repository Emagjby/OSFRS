using FluentAssertions;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.IntegrationTests.TestUtils.Builders;

namespace OSFRS.IntegrationTests.Auth;

public class UserService_IntegrationTests : IntegrationTestBase
{
    public UserService_IntegrationTests()
        : base("OSFRS_IT_IntegrationTests") { }

    // -------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------
    private IProfileService CreateService() => Factory.ProfileService();

    private IUserRepository UserRepo() => Factory.UserRepo();

    // =============================================================
    // GET PROFILE
    // =============================================================
    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile_WhenUserExists()
    {
        var user = UserBuilder
            .Create()
            .WithName("Gencho")
            .WithUsername("gencho123")
            .WithEmail("gencho@mail.com")
            .Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var service = CreateService();

        var result = await service.GetProfileAsync(user.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be("Gencho");
        result.Username.Should().Be("gencho123");
        result.Email.Should().Be("gencho@mail.com");
    }

    [Fact]
    public async Task GetProfileAsync_ShouldThrow_WhenUserNotFound()
    {
        var service = CreateService();

        var act = () => service.GetProfileAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // =============================================================
    // UPDATE PROFILE
    // =============================================================
    [Fact]
    public async Task UpdateProfileAsync_ShouldModifyNameUsernameEmail_WhenValid()
    {
        var user = UserBuilder
            .Create()
            .WithName("Old Name")
            .WithUsername("olduser")
            .WithEmail("old@mail.com")
            .Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var service = CreateService();

        var dto = new UpdatedProfileDto
        {
            Name = "New Name",
            Username = "newuser",
            Email = "new@mail.com",
        };

        await service.UpdateProfileAsync(user.Id, dto);

        var updated = await UserRepo().GetByIdAsync(user.Id);

        updated!.Name.Should().Be("New Name");
        updated.Username.Should().Be("newuser");
        updated.Email.Should().Be("new@mail.com");
        updated.UpdatedAt.Should().BeAfter(user.CreatedAt);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldFail_WhenUsernameAlreadyTaken()
    {
        var user1 = UserBuilder.Create().WithUsername("taken").Build();
        var user2 = UserBuilder.Create().WithUsername("freeuser").Build();

        await UserRepo().AddAsync(user1);
        await UserRepo().AddAsync(user2);
        await UserRepo().SaveChangesAsync();

        var service = CreateService();

        var dto = new UpdatedProfileDto { Username = "taken" };

        var act = () => service.UpdateProfileAsync(user2.Id, dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldFail_WhenEmailAlreadyTaken()
    {
        var user1 = UserBuilder.Create().WithEmail("a@mail.com").Build();
        var user2 = UserBuilder.Create().WithEmail("b@mail.com").Build();

        await UserRepo().AddAsync(user1);
        await UserRepo().AddAsync(user2);
        await UserRepo().SaveChangesAsync();

        var service = CreateService();

        var dto = new UpdatedProfileDto { Email = "a@mail.com" };

        var act = () => service.UpdateProfileAsync(user2.Id, dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldFail_WhenNameInvalid()
    {
        var user = UserBuilder.Create().Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var service = CreateService();

        var dto = new UpdatedProfileDto { Name = "Bad123" };

        var act = () => service.UpdateProfileAsync(user.Id, dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldFail_WhenUserDoesNotExist()
    {
        var service = CreateService();

        var dto = new UpdatedProfileDto { Name = "New Name" };

        var act = () => service.UpdateProfileAsync(999, dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // =============================================================
    // PARTIAL UPDATE
    // =============================================================
    [Fact]
    public async Task UpdateProfileAsync_ShouldAllowPartialUpdates()
    {
        var user = UserBuilder
            .Create()
            .WithName("Gencho")
            .WithUsername("gencho")
            .WithEmail("g@mail.com")
            .Build();

        await UserRepo().AddAsync(user);
        await UserRepo().SaveChangesAsync();

        var service = CreateService();

        var dto = new UpdatedProfileDto
        {
            Username = "updateduser",
            // Name + Email missing (partial)
        };

        await service.UpdateProfileAsync(user.Id, dto);

        var updated = await UserRepo().GetByIdAsync(user.Id);

        updated!.Username.Should().Be("updateduser");
        updated.Name.Should().Be("Gencho");
        updated.Email.Should().Be("g@mail.com");
    }
}
