using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Reservations;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

/// <summary>
/// Handles all reservation operations, including creation, updates,
/// cancellation, searching, availability calculation, and admin overrides.
/// </summary>
public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repo;
    private readonly IFacilityRepository _facility;
    private readonly IAppLogger<ReservationService> _logger;

    private readonly IValidator<(CreateReservationDto dto, int userId)> _createValidator;
    private readonly IValidator<(
        UpdateReservationDto dto,
        Reservation existing,
        bool isAdmin,
        int userId
    )> _updateValidator;
    private readonly CancelReservationValidator _cancelValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservationService"/> class.
    /// </summary>
    /// <param name="repo">Repository used for reservation persistence and querying.</param>
    /// <param name="facility">Repository used for facility existense check.</param>
    /// <param name="logger">Logging abstraction for reservation operations.</param>
    /// <param name="createValidator">Validator for reservation creation.</param>
    /// <param name="updateValidator">Validator for reservation updates.</param>
    /// <param name="cancelValidator">Validator ensuring cancellation rules are followed.</param>
    public ReservationService(
        IReservationRepository repo,
        IFacilityRepository facility,
        IAppLogger<ReservationService> logger,
        IValidator<(CreateReservationDto dto, int userId)> createValidator,
        IValidator<(
            UpdateReservationDto dto,
            Reservation existing,
            bool isAdmin,
            int userId
        )> updateValidator,
        CancelReservationValidator cancelValidator
    )
    {
        _repo = repo;
        _facility = facility;
        _logger = logger;

        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _cancelValidator = cancelValidator;
    }

    /// <summary>
    /// Returns the availability calendar for a facility for the specified day.
    /// </summary>
    /// <param name="facilityId">The facility whose availability is requested.</param>
    /// <param name="date">The target date. Defaults to today.</param>
    /// <returns>A collection of availability slots representing scheduled reservations.</returns>
    public async Task<IEnumerable<AvailabilitySlotDto>> GetAvailabilityCalendarAsync(
        int facilityId,
        DateTime? date = null
    )
    {
        if (await _facility.GetByIdAsync(facilityId) is null)
            throw new NotFoundException("Facility not found.");

        var targetDate = date ?? DateTime.UtcNow;
        var start = targetDate.Date;
        var end = start.AddDays(1);

        var reservations = await _repo.GetByFacilityAndRangeAsync(facilityId, start, end);
        var activeReservations = reservations.Where(r =>
            !string.Equals(r.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
        );

        return activeReservations.Select(r => new AvailabilitySlotDto
        {
            Id = r.Id,
            UserId = r.UserId,
            FacilityId = r.FacilityId,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Status = r.Status,
        });
    }

    /// <summary>
    /// Retrieves all reservations for a specific facility within an optional time range.
    /// </summary>
    public async Task<IEnumerable<Reservation>> GetReservationsAsync(
        int facilityId,
        DateTime? start = null,
        DateTime? end = null
    ) => await _repo.GetByFacilityAndRangeAsync(facilityId, start, end);

    /// <summary>
    /// Performs a flexible search across reservations using optional filters.
    /// </summary>
    public async Task<IEnumerable<Reservation>> SearchReservationAsync(
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    ) => await _repo.SearchAsync(userId, facilityId, start, end);

    /// <summary>
    /// Creates a reservation for the specified user.
    /// </summary>
    /// <param name="dto">Reservation data provided by the user.</param>
    /// <param name="userId">ID of the user creating the reservation.</param>
    /// <returns>The created reservation.</returns>
    public async Task<Reservation> CreateReservationAsync(CreateReservationDto dto, int userId)
    {
        _logger.LogInformation(
            "User {UserId} creating reservation for facility {FacilityId} from {Start} to {End}",
            userId,
            dto.FacilityId,
            dto.StartTime,
            dto.EndTime
        );

        await _createValidator.ValidateAsync((dto, userId));

        var reservation = new Reservation
        {
            UserId = userId,
            FacilityId = dto.FacilityId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repo.AddAsync(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Reservation {ReservationId} created for user {UserId}",
            reservation.Id,
            userId
        );

        return reservation;
    }

    /// <summary>
    /// Updates an existing reservation for the specified user.
    /// </summary>
    public async Task<Reservation> UpdateReservationAsync(
        int id,
        UpdateReservationDto dto,
        int userId
    )
    {
        var reservation =
            await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Reservation not found.");

        _logger.LogInformation("User {UserId} updating reservation {ReservationId}", userId, id);

        await _updateValidator.ValidateAsync((dto, reservation, isAdmin: false, userId));

        if (dto.StartTime.HasValue)
            reservation.StartTime = dto.StartTime.Value;

        if (dto.EndTime.HasValue)
            reservation.EndTime = dto.EndTime.Value;

        reservation.UpdatedAt = DateTime.UtcNow;

        _repo.Update(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} updated by user {UserId}", id, userId);

        return reservation;
    }

    /// <summary>
    /// Cancels a reservation if permitted for the requesting user.
    /// </summary>
    public async Task CancelReservationAsync(int id, int userId)
    {
        var reservation =
            await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Reservation not found.");

        _logger.LogInformation(
            "User {UserId} requested cancellation for reservation {ReservationId}",
            userId,
            id
        );

        await _cancelValidator.ValidateAsync(reservation, userId);

        reservation.Status = "Cancelled";
        reservation.UpdatedAt = DateTime.UtcNow;

        _repo.Update(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Reservation {ReservationId} cancelled by user {UserId}",
            id,
            userId
        );
    }

    /// <summary>
    /// Retrieves all reservations in the system.
    /// </summary>
    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync() =>
        await _repo.GetAllReadonlyAsync();

    /// <summary>
    /// Deletes a reservation as an administrator.
    /// </summary>
    public async Task DeleteReservationAsync(int id, int adminId)
    {
        _logger.LogInformation("Admin {AdminId} deleting reservation {ReservationId}", adminId, id);

        var reservation =
            await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Reservation not found.");

        _repo.Remove(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} deleted reservation {ReservationId}", adminId, id);
    }

    /// <summary>
    /// Updates a reservation with administrative privileges.
    /// </summary>
    public async Task<Reservation> AdminUpdateReservationAsync(
        int id,
        UpdateReservationDto dto,
        int adminId
    )
    {
        var reservation =
            await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Reservation not found.");

        _logger.LogInformation("Admin {AdminId} updating reservation {ReservationId}", adminId, id);

        await _updateValidator.ValidateAsync((dto, reservation, isAdmin: true, adminId));

        if (dto.StartTime.HasValue)
            reservation.StartTime = dto.StartTime.Value;

        if (dto.EndTime.HasValue)
            reservation.EndTime = dto.EndTime.Value;

        if (dto.Status is not null)
            reservation.Status = dto.Status;

        reservation.UpdatedAt = DateTime.UtcNow;

        _repo.Update(reservation);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} updated reservation {ReservationId}", adminId, id);

        return reservation;
    }
}
