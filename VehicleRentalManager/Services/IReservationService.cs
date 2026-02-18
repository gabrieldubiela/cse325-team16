using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public interface IReservationService
{
    Task<List<Reservation>> GetAsync();
    Task<List<Vehicle>> GetAvailableVehiclesAsync(DateTime start, DateTime end);
    Task CreateAsync(Reservation reservation);
    Task UpdateAsync(string id, Reservation reservation);
    Task UpdateStatusAsync(string id, ReservationStatus status);
    Task DeleteAsync(string id);
}
