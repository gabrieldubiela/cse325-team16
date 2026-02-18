using MongoDB.Driver;
using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public class ReservationService : IReservationService
{
    private readonly IMongoCollection<Reservation> _reservations;
    private readonly IClientService _clientService;
    private readonly IVehicleService _vehicleService;

    public ReservationService(
        MongoDbService mongoDbService,
        IClientService clientService,
        IVehicleService vehicleService)
        : this(
            mongoDbService.GetCollection<Reservation>("reservations"),
            clientService,
            vehicleService)
    {
    }

    public ReservationService(
        IMongoCollection<Reservation> reservations,
        IClientService clientService,
        IVehicleService vehicleService)
    {
        _reservations = reservations;
        _clientService = clientService;
        _vehicleService = vehicleService;
    }

    public async Task<List<Reservation>> GetAsync()
    {
        var reservations = await _reservations
            .Find(_ => true)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Perform application-side joins to populate related entities since MongoDB
        // doesn't support complex relational joins as efficiently or simply as SQL.
        foreach (var reservation in reservations)
        {
            reservation.Client = await _clientService.GetByIdAsync(reservation.ClientId);
            reservation.Vehicle = await _vehicleService.GetByIdAsync(reservation.VehicleId);
        }

        return reservations;
    }

    public async Task<Reservation?> GetByIdAsync(string id)
    {
        var reservation = await _reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (reservation != null)
        {
            reservation.Client = await _clientService.GetByIdAsync(reservation.ClientId);
            reservation.Vehicle = await _vehicleService.GetByIdAsync(reservation.VehicleId);
        }
        return reservation;
    }

    public async Task<List<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
    {
        var allVehicles = await _vehicleService.GetAsync();
        var availableVehicles = new List<Vehicle>();

        foreach (var vehicle in allVehicles.Where(v => v.IsAvailable))
        {
            bool isAvailable = await IsVehicleAvailableAsync(vehicle.Id!, startDate, endDate);
            if (isAvailable)
            {
                availableVehicles.Add(vehicle);
            }
        }

        return availableVehicles;
    }

    public async Task<bool> IsVehicleAvailableAsync(string vehicleId, DateTime startDate, DateTime endDate, string? excludeReservationId = null)
    {
        // Check for any active reservation that overlaps with the requested date range.
        // Overlap logic covers: start inside, end inside, or enveloping the requested range.
        var filter = Builders<Reservation>.Filter.And(
            Builders<Reservation>.Filter.Eq(r => r.VehicleId, vehicleId),
            Builders<Reservation>.Filter.Eq(r => r.Status, ReservationStatus.Active),
            Builders<Reservation>.Filter.Or(
                // New reservation starts during existing reservation
                Builders<Reservation>.Filter.And(
                    Builders<Reservation>.Filter.Lte(r => r.StartDate, startDate),
                    Builders<Reservation>.Filter.Gte(r => r.EndDate, startDate)
                ),
                // New reservation ends during existing reservation
                Builders<Reservation>.Filter.And(
                    Builders<Reservation>.Filter.Lte(r => r.StartDate, endDate),
                    Builders<Reservation>.Filter.Gte(r => r.EndDate, endDate)
                ),
                // New reservation completely contains existing reservation
                Builders<Reservation>.Filter.And(
                    Builders<Reservation>.Filter.Gte(r => r.StartDate, startDate),
                    Builders<Reservation>.Filter.Lte(r => r.EndDate, endDate)
                )
            )
        );

        if (!string.IsNullOrEmpty(excludeReservationId))
        {
            filter = Builders<Reservation>.Filter.And(
                filter,
                Builders<Reservation>.Filter.Ne(r => r.Id, excludeReservationId)
            );
        }

        var conflictingReservation = await _reservations.Find(filter).FirstOrDefaultAsync();
        return conflictingReservation == null;
    }

    public async Task CreateAsync(Reservation reservation)
    {
        // Set creation time server-side to ensure audit trail integrity.
        reservation.CreatedAt = DateTime.UtcNow;
        await _reservations.InsertOneAsync(reservation);

        // Update client's activity log asynchronously to keep the client record fresh.
        await _clientService.UpdateLastRentalDateAsync(reservation.ClientId);
    }

    public async Task UpdateAsync(string id, Reservation reservation)
    {
        await _reservations.ReplaceOneAsync(r => r.Id == id, reservation);
    }

    public async Task DeleteAsync(string id)
    {
        await _reservations.DeleteOneAsync(r => r.Id == id);
    }

    public async Task UpdateStatusAsync(string id, ReservationStatus status)
    {
        var update = Builders<Reservation>.Update.Set(r => r.Status, status);
        await _reservations.UpdateOneAsync(r => r.Id == id, update);
    }
}