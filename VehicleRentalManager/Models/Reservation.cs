using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManager.Models;

// Represents a single vehicle rental event.
public class Reservation
{
    [BsonId]
    // Store the BSON ObjectId as a string for easier use in C# and API responses.
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // Use IDs for relationships to keep documents decoupled, following NoSQL best practices.
    [Required(ErrorMessage = "Client is required")]
    public string ClientId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vehicle is required")]
    public string VehicleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    // Calculated at the time of reservation to lock in the price.
    public decimal TotalPrice { get; set; }

    // Manages the lifecycle of the reservation (e.g., active, completed).
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    // These navigation properties are populated at runtime for convenience in the UI.
    // They are marked with [BsonIgnore] to prevent MongoDB from trying to store them,
    // which would create redundant data and potential circular dependencies.
    [BsonIgnore]
    public Client? Client { get; set; }

    [BsonIgnore]
    public Vehicle? Vehicle { get; set; }
}

public enum ReservationStatus
{
    Active,
    Completed,
    Cancelled
}