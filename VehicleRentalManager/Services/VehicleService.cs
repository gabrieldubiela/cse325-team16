using MongoDB.Driver;
using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public class VehicleService : IVehicleService
{
    private readonly IMongoCollection<Vehicle> _vehicles;

    // Inject MongoContext to abstract the database connection details.
    public VehicleService(MongoContext context)
    {
        _vehicles = context.Database.GetCollection<Vehicle>("Vehicle");
    }

    public async Task<List<Vehicle>> GetAllAsync()
    {
        return await _vehicles.Find(_ => true).ToListAsync();
    }
    public async Task<List<Vehicle>> GetAsync()
    {
        return await _vehicles.Find(_ => true).ToListAsync();
    }


    public async Task CreateAsync(Vehicle vehicle)
    {
        await _vehicles.InsertOneAsync(vehicle);
    }

    public async Task DeleteAsync(string id)
    {
        await _vehicles.DeleteOneAsync(v => v.Id == id);
    }

    public async Task<Vehicle> GetByIdAsync(string id)
    {
        return await _vehicles.Find(v => v.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(string id, Vehicle vehicle)
    {
        await _vehicles.ReplaceOneAsync(v => v.Id == id, vehicle);
    }

}
