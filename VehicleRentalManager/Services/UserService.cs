using MongoDB.Driver;
using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<AppUser> _users;

    public UserService(MongoDbService mongoDbService)
    {
        _users = mongoDbService.GetCollection<AppUser>("users");
    }

    public async Task<AppUser?> FindByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

        public async Task<List<AppUser>> GetAllAsync()
    {
        return await _users.Find(_ => true).SortBy(u => u.CreatedAt).ToListAsync();
    }

    public async Task<List<AppUser>> GetPendingAsync()
    {
        return await _users.Find(u => !u.IsApproved).SortBy(u => u.CreatedAt).ToListAsync();
    }

    public async Task<AppUser> CreateAsync(string name, string email, string googleId)
    {
        var user = new AppUser
        {
            Name      = name,
            Email     = email,
            GoogleId  = googleId,
            // Default to unapproved to prevent unauthorized access immediately after OAuth signup.
            IsApproved = false,
            CreatedAt  = DateTime.UtcNow
        };
        await _users.InsertOneAsync(user);
        return user;
    }

        public async Task ApproveAsync(string id)
    {
        var update = Builders<AppUser>.Update.Set(u => u.IsApproved, true);
        await _users.UpdateOneAsync(u => u.Id == id, update);
    }

    public async Task RevokeAsync(string id)
    {
        var update = Builders<AppUser>.Update.Set(u => u.IsApproved, false);
        await _users.UpdateOneAsync(u => u.Id == id, update);
    }

    public async Task UpdateLastLoginAsync(string id)
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.LastLoginAt, DateTime.UtcNow);
        await _users.UpdateOneAsync(u => u.Id == id, update);
    }
}