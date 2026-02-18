using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public interface IClientService
{
    Task<List<Client>> GetAsync();
    Task<Client?> GetByIdAsync(string id);
    Task<Client?> GetByEmailAsync(string email);
    Task CreateAsync(Client client);
    Task UpdateAsync(string id, Client client);
    Task DeleteAsync(string id);

    Task UpdateLastRentalDateAsync(string id);
}
