using FluentAssertions;
using Moq;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _repo;
    private readonly Mock<IFacilityRepository> _facilityRepo;
    private readonly Mock<IAppLogger<ReservationService>> _logger;

    private readonly Mock<IValidator<(CreateReservationDto dto, int userId)>> _createValidator;
    private readonly Mock<
        IValidator<(UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId)>
    > _updateValidator;
    private readonly Mock<IValidator<(Reservation reservation, int userId)>> _cancelValidator;

    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _repo = MockFactories.ReservationRepo();
        _facilityRepo = MockFactories.FacilityRepo();
        _logger = MockFactories.Logger<ReservationService>();
        _createValidator = MockFactories.Validator<(CreateReservationDto dto, int userId)>();
        _updateValidator = MockFactories.Validator<(
            UpdateReservationDto dto,
            Reservation existing,
            bool isAdmin,
            int userId
        )>();
        _cancelValidator = MockFactories.Validator<(Reservation reservation, int userId)>();

        _service = new ReservationService(
            _repo.Object,
            _facilityRepo.Object,
            _logger.Object,
            _createValidator.Object,
            _updateValidator.Object,
            _cancelValidator.Object
        );
    }

    // ============================================================
    // CREATE -> Validator is called
    // ============================================================

    [Fact]
    public async Task Create_ShouldCallCreateValidator()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facilityRepo
            .Setup(x => x.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Facility { Id = dto.FacilityId });

        var USER_ID = 10;
        await _service.CreateReservationAsync(dto, USER_ID);

        var expected = (dto, USER_ID);
        _createValidator.Verify(v => v.ValidateAsync(expected), Times.Once);
    }

    // ============================================================
    // CREATE -> Repo.Add + SaveChanges
    // ============================================================

    [Fact]
    public async Task Create_ShouldAddReservation_AndSave()
    {
        var dto = FakeData.CreateReservationDto().Generate();

        _facilityRepo
            .Setup(x => x.GetByIdAsync(dto.FacilityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Facility { Id = dto.FacilityId });

        var USER_ID = 20;
        await _service.CreateReservationAsync(dto, USER_ID);

        _repo.Verify(
            r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // GETBYID NULL -> NotFoundException
    // ============================================================

    [Fact]
    public async Task Update_ShouldThrowNotFound_WhenReservationMissing()
    {
        var MISSING_RESERVATION_ID = 123;
        _repo
            .Setup(x => x.GetByIdAsync(MISSING_RESERVATION_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var dto = new UpdateReservationDto();

        var USER_ID = 1;
        var act = async () =>
            await _service.UpdateReservationAsync(MISSING_RESERVATION_ID, dto, USER_ID);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // UPDATE -> Validator called with correct tuple
    // ============================================================

    [Fact]
    public async Task Update_ShouldCallUpdateValidator_WithCorrectTuple()
    {
        var USER_ID = 1;
        var dto = FakeData.UpdateReservationDto().Generate();

        var existing = FakeData.Reservation().Generate();
        existing.UserId = USER_ID;

        _repo
            .Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _service.UpdateReservationAsync(existing.Id, dto, USER_ID);

        var IS_ADMIN = false;
        var expected = (dto, existing, IS_ADMIN, USER_ID);

        _updateValidator.Verify(v => v.ValidateAsync(expected), Times.Once);
    }

    // ============================================================
    // UPDATE modifies only supplied fields
    // ============================================================

    [Fact]
    public async Task Update_ShouldModifyOnlyProvidedFields()
    {
        var USER_ID = 1;
        var reservation = FakeData.Reservation().Generate();

        reservation.UserId = USER_ID;
        reservation.StartTime = DateTime.UtcNow.AddHours(2);
        reservation.EndTime = DateTime.UtcNow.AddHours(4);

        var dto = new UpdateReservationDto { StartTime = DateTime.UtcNow.AddHours(10) };

        _repo
            .Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _service.UpdateReservationAsync(reservation.Id, dto, USER_ID);

        var NEW_START_TIME = dto.StartTime;
        var OLD_END_TIME = reservation.EndTime;

        reservation.StartTime.Should().Be(NEW_START_TIME);
        reservation.EndTime.Should().Be(OLD_END_TIME);

        _repo.Verify(r => r.Update(reservation), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // AdminUpdate -> validator called with isAdmin=true
    // ============================================================

    [Fact]
    public async Task AdminUpdate_ShouldCallValidatorWithIsAdminTrue()
    {
        var reservation = FakeData.Reservation().Generate();
        var dto = FakeData.UpdateReservationDto().Generate();

        int ADMIN_ID = 99;

        _repo
            .Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _service.AdminUpdateReservationAsync(reservation.Id, dto, ADMIN_ID);

        var IS_ADMIN = true;
        var expected = (dto, reservation, IS_ADMIN, ADMIN_ID);

        _updateValidator.Verify(v => v.ValidateAsync(expected), Times.Once);
    }

    // ============================================================
    // AdminUpdate modifies Status
    // ============================================================

    [Fact]
    public async Task AdminUpdate_ShouldUpdateStatus_WhenProvided()
    {
        var reservation = FakeData.Reservation().Generate();
        reservation.Status = "Pending";

        var dto = new UpdateReservationDto { Status = "Confirmed" };

        int ADMIN_ID = 99;

        _repo
            .Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _service.AdminUpdateReservationAsync(reservation.Id, dto, ADMIN_ID);

        reservation.Status.Should().Be("Confirmed");

        _repo.Verify(r => r.Update(reservation), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // CANCEL -> cancelValidator called
    // ============================================================

    [Fact]
    public async Task Cancel_ShouldCallCancelValidator()
    {
        var reservation = FakeData.Reservation().Generate();
        var USER_ID = reservation.UserId;

        _repo
            .Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _service.CancelReservationAsync(reservation.Id, USER_ID);

        var expected = (reservation, USER_ID);

        _cancelValidator.Verify(v => v.ValidateAsync(expected), Times.Once);
    }

    // ============================================================
    // CANCEL -> sets status + save
    // ============================================================

    [Fact]
    public async Task Cancel_ShouldSetStatusCancelled_AndSave()
    {
        var reservation = FakeData.Reservation().Generate();
        reservation.Status = "Pending";

        var USER_ID = reservation.UserId;

        _repo
            .Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        await _service.CancelReservationAsync(reservation.Id, USER_ID);

        reservation.Status.Should().Be("Cancelled");

        _repo.Verify(r => r.Update(reservation), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ============================================================
    // Query passthrough tests
    // ============================================================

    [Fact]
    public async Task GetReservations_ShouldReturnRepoResults()
    {
        var FACILITY_ID = 1;

        var list = FakeData.Reservation().RuleFor(r => r.FacilityId, _ => FACILITY_ID).Generate(3);

        DateTime? START = null;
        DateTime? END = null;

        _repo.Setup(r => r.GetByFacilityAndRangeAsync(FACILITY_ID, START, END)).ReturnsAsync(list);

        var result = await _service.GetReservationsAsync(FACILITY_ID);

        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task Search_ShouldReturnRepoResults()
    {
        var USER_ID = 1;

        var list = FakeData.Reservation().RuleFor(r => r.UserId, _ => USER_ID).Generate(2);

        int? FACILITY_ID = null;
        DateTime? START = null;
        DateTime? END = null;

        _repo.Setup(r => r.SearchAsync(USER_ID, FACILITY_ID, START, END)).ReturnsAsync(list);

        var result = await _service.SearchReservationAsync(USER_ID);

        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetAll_ShouldReturnRepoResults()
    {
        var list = FakeData.Reservation().Generate(5);

        _repo.Setup(r => r.GetAllReadonlyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _service.GetAllReservationsAsync();

        result.Should().BeEquivalentTo(list);
    }
}
