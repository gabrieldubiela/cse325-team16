using MongoDB.Driver;
using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public class ClientService : IClientService
{
    private readonly IMongoCollection<Client> _clients;

    public ClientService(IMongoCollection<Client> clients)
    {
        _clients = clients;
    }

    public ClientService(MongoDbService mongoDbService)
        : this(mongoDbService.GetCollection<Client>("clients"))
    {
    }

    public async Task<List<Client>> GetAsync()
    {
        // Sort clients by name to ensure a consistent order in dropdowns and lists across the application.
        return await _clients.Find(_ => true).SortBy(c => c.Name).ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(string id)
    {
        return await _clients.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Client?> GetByEmailAsync(string email)
    {
        return await _clients.Find(c => c.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Client client)
    {
        // Enforce server-side timestamp generation to prevent client clock skew issues.
        client.CreatedAt = DateTime.UtcNow;
        await _clients.InsertOneAsync(client);
    }

    public async Task UpdateAsync(string id, Client client)
    {
        await _clients.ReplaceOneAsync(c => c.Id == id, client);
    }

    public async Task DeleteAsync(string id)
    {
        await _clients.DeleteOneAsync(c => c.Id == id);
    }

    public async Task UpdateLastRentalDateAsync(string id)
    {
        // Targeted update for operational data to avoid overwriting user-editable fields during rental processing.
        var update = Builders<Client>.Update.Set(c => c.LastRentalDate, DateTime.UtcNow);
        await _clients.UpdateOneAsync(c => c.Id == id, update);
    }
}