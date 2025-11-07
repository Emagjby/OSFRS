using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;
using OSFRS.Backend.Validators;

namespace OSFRS.Backend.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repo;
    private readonly IAppLogger<ReservationService> _logger;

    public ReservationService(IReservationRepository repo, IAppLogger<ReservationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<AvailabilitySlotDto>> GetAvailabilityCalendarAsync(int facilityId, DateTime? date = null)
    {
        try
        {
            var targetDate = date ?? DateTime.UtcNow;
            var start = targetDate.Date;
            var end = start.AddDays(1);

            var reservations = await _repo.GetByFacilityAndRangeAsync(facilityId, start, end);
            var calendar = reservations.Select(reservation => new AvailabilitySlotDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                FacilityId = reservation.FacilityId,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = reservation.Status
            });

            _logger.LogInformation("Generated availability calendar for facility {FacilityId} on {Date}", facilityId, targetDate);
            return calendar;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error generating calendar for facility {FacilityId}", facilityId);
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(int facilityId, DateTime? start = null, DateTime? end = null)
    {
        try
        {
            var reservations = await _repo.GetByFacilityAndRangeAsync(facilityId, start, end);
            _logger.LogInformation("Fetched {Count} reservations for facility {FacilityId}", reservations.Count(), facilityId);
            return reservations;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching reservations for facility {FacilityId}", facilityId);
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> SearchReservationAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null)
    {
        try
        {
            var results = await _repo.SearchAsync(userId, facilityId, start, end);
            _logger.LogInformation("Search found {Count} reservations (filters: user={UserId} facility={FacilityId})", results.Count(), userId!, facilityId!);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching reservations");
            throw;
        }
    }
    
    public async Task<Reservation> CreateReservationAsync(CreateReservationDto dto)
    {
        _logger.LogInformation("Creating reservation for user {UserId} at facility {FacilityId}", dto.UserId, dto.FacilityId);

        if (!ReservationValidator.ValidateFacilityId(dto.FacilityId)) throw new ArgumentException("Invalid facility ID.");
        if (!ReservationValidator.ValidateUserId(dto.UserId)) throw new ArgumentException("Invalid user ID.");
        if (!ReservationValidator.ValidateTimes(dto.StartTime, dto.EndTime)) throw new ArgumentException("Invalid time range for reservations");

        bool isAvailable = await _repo.IsSlotAvailableAsync(dto.StartTime, dto.EndTime, dto.FacilityId);
        if (!isAvailable) throw new InvalidOperationException("The selected time slot is not available.");

        var reservation = new Reservation
        {
            UserId = dto.UserId,
            FacilityId = dto.FacilityId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(reservation);
        _logger.LogInformation("Reservation {ReservationId} created successfully", reservation.Id);

        return reservation;
    }

    public async Task<Reservation> UpdateReservationAsync(int id, UpdateReservationDto dto, int userId)
    {
        try
        {
            var reservation = await _repo.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("User {UserId} attempted to update non-existant reservation {ReservationId}", userId, id);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update reservation {ReservationId} without permission", userId, id);
                throw new UnauthorizedAccessException("You do not have permission to update this reservation.");
            }

            if (!ReservationValidator.ValidateTimes(dto.StartTime, dto.EndTime)) 
                throw new ArgumentException("Invalid time range for reservation.");

            var isAvailable = await _repo.IsSlotAvailableAsync(dto.StartTime, dto.EndTime, reservation.FacilityId);
            if (!isAvailable) throw new InvalidOperationException("The selected time slot is already taken.");

            reservation.StartTime = dto.StartTime;
            reservation.EndTime = dto.EndTime;
            reservation.Status = dto.Status ?? reservation.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(reservation);
            _logger.LogInformation("User {UserId} updated reservation {ReservationId}", userId, id);

            return reservation;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation {ReservationId} by user {UserId}", id, userId);
            throw;
        }
    }

    public async Task CancelReservationAsync(int id, int userId)
    {
        try
        {
            var reservation = await _repo.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("User {UserId} attempted to cancel non-existant reservation {ReservationId}", userId, id);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to cancel reservation {ReservationId} without permission", userId, id);
                throw new UnauthorizedAccessException("You do not have permission to cancel this reservation.");
            }

            if (reservation.Status == "Cancelled")
            {
                _logger.LogWarning("Reservation {ReservationId} is already canceled.", id);
                throw new InvalidOperationException("This reservation is already canceled.");
            }

            await _repo.UpdateStatusAsync(id, "Cancelled");

            _logger.LogInformation("User {UserId} successfully canceled reservation {ReservationId}", userId, id);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId} by user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
    {
        try
        {
            var reservations = await _repo.GetAllAsync();
            _logger.LogInformation("Admin fetched all {Count} reservations.", reservations.Count());
            return reservations;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all reservations (admin).");
            throw;
        }
    }

    public async Task DeleteReservationAsync(int id, int adminId)
    {
        try
        {
            var reservation = await _repo.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Admin {AdminId} attempted to delete a non-existant reservation {ReservationId}", adminId, id);
                throw new InvalidOperationException("Reservation not found.");
            }

            await _repo.DeleteAsync(id);
            _logger.LogInformation("Admin {AdminId} deleted reservation {ReservationId}", adminId, id);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting reservation {ReservationId} by admin {AdminId}", id, adminId);
            throw;
        }
    }

    public async Task<Reservation> AdminUpdateReservationAsync(int id, UpdateReservationDto dto, int adminId)
    {
        try
        {
            var reservation = await _repo.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Admin {AdminId} attempted to update a non-existant reservation {ReservationId}", adminId, id);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (!ReservationValidator.ValidateTimes(dto.StartTime, dto.EndTime))
                throw new ArgumentException("Invalid time range for reservation.");

            bool hasConflict = await _repo.HasConflictAsync(reservation.FacilityId, dto.StartTime, dto.EndTime, id);
            if (hasConflict)
            {
                _logger.LogWarning("Admin {AdminId} is overriding a conflict while updating reservation {ReservationId}", adminId, id);
            }

            reservation.StartTime = dto.StartTime;
            reservation.EndTime = dto.EndTime;
            reservation.Status = dto.Status ?? reservation.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(reservation);
            _logger.LogInformation("Admin {AdminId} updated reservation {ReservationId}", adminId, id);

            return reservation;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation {ReservationId} by admin {AdminId}.", id, adminId);
            throw;
        }
    }
}