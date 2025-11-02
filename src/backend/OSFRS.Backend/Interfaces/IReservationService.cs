using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetReservationsAsync(int facilityId, DateTime? start = null, DateTime? end = null);
    Task<IEnumerable<AvailabilitySlotDto>> GetAvailabilityCalendarAsync(int facilityId, DateTime? date = null);
    Task<IEnumerable<Reservation>> SearchReservationAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null);
    Task<Reservation> CreateReservationAsync(CreateReservationDto dto);
    Task<Reservation> UpdateReservationAsync(int id, UpdateReservationDto dto, int userId);
    Task CancelReservationAsync(int id, int userId);
}