using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VehicleRentalManager.Models;

// Represents an application user, authenticated via an external provider like Google.
public class AppUser
{
    [BsonId]
    // Store the BSON ObjectId as a string for easier use in C# and API responses.
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // The unique identifier from the external authentication provider (e.g., Google's 'sub' claim).
    public string GoogleId { get; set; } = string.Empty;

    // A security flag; administrators must manually approve new users before they can access the system.
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}