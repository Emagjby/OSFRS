using OSFRS.Backend.DTOs.Reports;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Service;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetReservationsAsync(int facilityId, DateTime? start = null, DateTime? end = null);
    Task<IEnumerable<AvailabilitySlotDto>> GetAvailabilityCalendarAsync(int facilityId, DateTime? date = null);
    Task<IEnumerable<Reservation>> SearchReservationAsync(int? userId = null, int? facilityId = null, DateTime? start = null, DateTime? end = null);
    Task<Reservation> CreateReservationAsync(CreateReservationDto dto, int userId);
    Task<Reservation> UpdateReservationAsync(int id, UpdateReservationDto dto, int userId);
    Task CancelReservationAsync(int id, int userId);
    Task<IEnumerable<Reservation>> GetAllReservationsAsync();
    Task DeleteReservationAsync(int id, int adminId);
    Task<Reservation> AdminUpdateReservationAsync(int id, UpdateReservationDto dto, int adminId);
}