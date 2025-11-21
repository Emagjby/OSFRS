using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Validators.Reservations;

namespace OSFRS.Backend.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repo;
    private readonly IAppLogger<ReservationService> _logger;

    private readonly IValidator<(CreateReservationDto dto, int userId)> _createValidator;
    private readonly IValidator<(UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId)> _updateValidator;
    private readonly CancelReservationValidator _cancelValidator;

    public ReservationService(
        IReservationRepository repo,
        IAppLogger<ReservationService> logger,
        IValidator<(CreateReservationDto dto, int userId)> createValidator,
        IValidator<(UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId)> updateValidator,
        CancelReservationValidator cancelValidator
    )
    {
        _repo = repo;
        _logger = logger;

        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _cancelValidator = cancelValidator;
    }

    public async Task<IEnumerable<AvailabilitySlotDto>> GetAvailabilityCalendarAsync(int facilityId, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var start = targetDate.Date;
        var end = start.AddDays(1);

        var reservations = await _repo.GetByFacilityAndRangeAsync(facilityId, start, end);

        return reservations.Select(r => new AvailabilitySlotDto
        {
            Id = r.Id,
            UserId = r.UserId,
            FacilityId = r.FacilityId,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Status = r.Status
        });
    }

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(int facilityId, DateTime? start = null, DateTime? end = null)
        => await _repo.GetByFacilityAndRangeAsync(facilityId, start, end);

    public async Task<IEnumerable<Reservation>> SearchReservationAsync(
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    ) => await _repo.SearchAsync(userId, facilityId, start, end);

    public async Task<Reservation> CreateReservationAsync(CreateReservationDto dto, int userId)
    {
        _logger.LogInformation(
            "User {UserId} creating reservation for facility {FacilityId} from {Start} to {End}",
            userId, dto.FacilityId, dto.StartTime, dto.EndTime);

        await _createValidator.ValidateAsync((dto, userId));

        var reservation = new Reservation
        {
            UserId = userId,
            FacilityId = dto.FacilityId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Reservation {ReservationId} created for user {UserId}",
            reservation.Id, userId);

        return reservation;
    }

    public async Task<Reservation> UpdateReservationAsync(int id, UpdateReservationDto dto, int userId)
    {
        var reservation = await _repo.GetByIdAsync(id)
                            ?? throw new NotFoundException("Reservation not found.");

        _logger.LogInformation(
            "User {UserId} updating reservation {ReservationId}",
            userId, id);

        await _updateValidator.ValidateAsync((dto, reservation, isAdmin: false, userId));

        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Status = dto.Status ?? reservation.Status;
        reservation.UpdatedAt = DateTime.UtcNow;

        _repo.Update(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Reservation {ReservationId} updated by user {UserId}",
            id, userId);

        return reservation;
    }

    public async Task CancelReservationAsync(int id, int userId)
    {
        var reservation = await _repo.GetByIdAsync(id)
                            ?? throw new NotFoundException("Reservation not found.");

        _logger.LogInformation(
            "User {UserId} requested cancellation for reservation {ReservationId}",
            userId, id);

        await _cancelValidator.ValidateAsync(reservation, userId);

        reservation.Status = "Cancelled";
        reservation.UpdatedAt = DateTime.UtcNow;

        _repo.Update(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Reservation {ReservationId} cancelled by user {UserId}",
            id, userId);
    }

    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        => await _repo.GetAllAsync();

    public async Task DeleteReservationAsync(int id, int adminId)
    {
        _logger.LogInformation(
            "Admin {AdminId} deleting reservation {ReservationId}",
            adminId, id);

        var reservation = await _repo.GetByIdAsync(id)
                            ?? throw new NotFoundException("Reservation not found.");

        _repo.Remove(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Admin {AdminId} deleted reservation {ReservationId}",
            adminId, id);
    }

    public async Task<Reservation> AdminUpdateReservationAsync(int id, UpdateReservationDto dto, int adminId)
    {
        var reservation = await _repo.GetByIdAsync(id)
                           ?? throw new NotFoundException("Reservation not found.");

        _logger.LogInformation(
            "Admin {AdminId} updating reservation {ReservationId}",
            adminId, id);

        await _updateValidator.ValidateAsync((dto, reservation, isAdmin: true, adminId));

        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Status = dto.Status ?? reservation.Status;
        reservation.UpdatedAt = DateTime.UtcNow;

        _repo.Update(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Admin {AdminId} updated reservation {ReservationId}",
            adminId, id);

        return reservation;
    }
}