using FluentAssertions;
using Moq;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class FacilityServiceTests
{
    private readonly Mock<IFacilityRepository> _repo;
    private readonly Mock<IAppLogger<FacilityService>> _logger;

    private readonly Mock<IValidator<CreateFacilityDto>> _createValidator;
    private readonly Mock<IUpdateValidator<UpdateFacilityDto, Facility>> _updateValidator;
    private readonly Mock<
        IValidator<(Facility facility, bool newAvailability)>
    > _availabilityValidator;

    private readonly FacilityService _service;

    public FacilityServiceTests()
    {
        _repo = MockFactories.FacilityRepo();
        _logger = MockFactories.Logger<FacilityService>();

        _createValidator = MockFactories.Validator<CreateFacilityDto>();
        _updateValidator = MockFactories.UpdateValidator<UpdateFacilityDto, Facility>();
        _availabilityValidator = MockFactories.Validator<(
            Facility facility,
            bool newAvailability
        )>();

        _service = new FacilityService(
            _repo.Object,
            _logger.Object,
            _createValidator.Object,
            _updateValidator.Object,
            _availabilityValidator.Object
        );
    }

    // ============================================================
    // CREATE
    // ============================================================

    [Fact]
    public async Task Create_ShouldCallCreateValidator()
    {
        var dto = FakeData.CreateFacilityDto().Generate();

        await _service.CreateAsync(dto);

        _createValidator.Verify(v => v.ValidateAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldAddAndSave()
    {
        var dto = FakeData.CreateFacilityDto().Generate();

        await _service.CreateAsync(dto);

        _repo.Verify(
            r => r.AddAsync(It.IsAny<Facility>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    [Fact]
    public async Task Update_ShouldThrowNotFound_WhenMissing()
    {
        var INVALID_ID = 999;
        var dto = FakeData.UpdateFacilityDto().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(INVALID_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var act = async () => await _service.UpdateAsync(INVALID_ID, dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ShouldCallValidator()
    {
        var facility = FakeData.Facility().Generate();
        var dto = FakeData.UpdateFacilityDto().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        await _service.UpdateAsync(facility.Id, dto);

        _updateValidator.Verify(v => v.ValidateAsync(dto, facility), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldModifyOnlyProvidedFields()
    {
        var facility = FakeData.Facility().Generate();
        facility.Name = "OldName";
        facility.Type = "OldType";
        facility.Capacity = 100;
        facility.Status = "Available";

        var dto = new UpdateFacilityDto
        {
            Name = "NewName",
            Capacity = 200,
            Status = null, // unchanged
            Type = null, // unchanged
        };

        _repo
            .Setup(r => r.GetByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        await _service.UpdateAsync(facility.Id, dto);

        facility.Name.Should().Be("NewName");
        facility.Capacity.Should().Be(200);
        facility.Status.Should().Be("Available");
        facility.Type.Should().Be("OldType");

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // AVAILABILITY UPDATE
    // ============================================================

    [Fact]
    public async Task UpdateAvailability_ShouldThrowNotFound_WhenMissing()
    {
        var INVALID_ID = 999;

        _repo
            .Setup(r => r.GetByIdAsync(INVALID_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var act = async () => await _service.UpdateAvailabilityAsync(INVALID_ID, true);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAvailability_ShouldCallAvailabilityValidator()
    {
        var facility = FakeData.Facility().Generate();
        var IS_AVAILABLE = true;

        _repo
            .Setup(r => r.GetByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        await _service.UpdateAvailabilityAsync(facility.Id, IS_AVAILABLE);

        var expected = (facility, IS_AVAILABLE);
        _availabilityValidator.Verify(v => v.ValidateAsync(expected), Times.Once);
    }

    [Fact]
    public async Task UpdateAvailability_ShouldUpdateRepositoryAndSave()
    {
        var facility = FakeData.Facility().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        await _service.UpdateAvailabilityAsync(facility.Id, false);

        _repo.Verify(r => r.UpdateAvailabilityAsync(facility.Id, false), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // DELETE
    // ============================================================

    [Fact]
    public async Task Delete_ShouldReturnFalse_WhenMissing()
    {
        var INVALID = 999;

        _repo
            .Setup(r => r.GetByIdAsync(INVALID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var result = await _service.DeleteAsync(INVALID);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ShouldReturnTrue_AndSave()
    {
        var facility = FakeData.Facility().Generate();

        _repo
            .Setup(r => r.GetByIdAsync(facility.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var result = await _service.DeleteAsync(facility.Id);

        result.Should().BeTrue();
        _repo.Verify(r => r.Remove(facility), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
