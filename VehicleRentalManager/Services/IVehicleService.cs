using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public interface IVehicleService
{
    Task<List<Vehicle>> GetAsync();
    Task<Vehicle?> GetByIdAsync(string id);
    Task CreateAsync(Vehicle vehicle);
    Task UpdateAsync(string id, Vehicle vehicle);
    Task DeleteAsync(string id);
}
