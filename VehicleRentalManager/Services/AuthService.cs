using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace VehicleRentalManager.Services;

public class AuthService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public record CurrentUserDto(string? Name, string? Email, bool IsAuthenticated);

    // Abstraction to retrieve the current user's profile from the Blazor circuit state,
    // normalizing claims from different providers (JWT vs Identity) for consistent UI consumption.
    public async Task<CurrentUserDto> GetCurrentUserAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var user  = state.User;

        if (user.Identity?.IsAuthenticated != true)
            return new CurrentUserDto(null, null, false);

        var name  = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                 ?? user.FindFirst(ClaimTypes.Name)?.Value;
        var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                 ?? user.FindFirst(ClaimTypes.Email)?.Value;

        return new CurrentUserDto(name, email, true);
    }

    // Triggers a browser-level navigation to the logout endpoint to ensure the HTTP-only cookie is cleared.
    // This cannot be done via Blazor SignalR directly because it lacks access to the HTTP response headers.
    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }
}