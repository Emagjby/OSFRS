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
            Status = "Pending...",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(reservation);
        _logger.LogInformation("Reservation {ReservationId} created successfully", reservation.Id);

        return reservation;
    }
}