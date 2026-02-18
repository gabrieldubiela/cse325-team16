using VehicleRentalManager.Models;

namespace VehicleRentalManager.Services;

public interface IUserService
{
    Task<AppUser?> FindByEmailAsync(string email);
    Task<List<AppUser>> GetAllAsync();
    Task<List<AppUser>> GetPendingAsync();
    Task<AppUser> CreateAsync(string name, string email, string googleId);
    Task ApproveAsync(string id);
    Task RevokeAsync(string id);
    Task UpdateLastLoginAsync(string id);
}
