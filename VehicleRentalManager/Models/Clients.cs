using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalManager.Models;

// Represents a customer who can rent vehicles.
public class Client
{
    [BsonId]
    // Store the BSON ObjectId as a string for better compatibility across layers (e.g., JSON serialization).
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Driver License is required")]
    public string DriverLicense { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Tracks the last rental activity, useful for reporting or identifying dormant clients.
    public DateTime? LastRentalDate { get; set; }

    // A soft-delete flag. Instead of deleting clients, we mark them as inactive to preserve historical data.
    public bool IsActive { get; set; } = true;
}