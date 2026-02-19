using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace VehicleRentalManager.Services;

// Bridges the gap between ASP.NET Core's HTTP context (cookies) and Blazor's SignalR circuit.
// We manually extract the JWT from the cookie because standard Identity state doesn't automatically flow to the Blazor circuit.
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthenticationState _cachedState;

    public JwtAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService)
    {
        // Capture the auth state immediately during the initial HTTP request.
        // This is the only moment HttpContext is guaranteed to be available.
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext != null &&
            httpContext.Request.Cookies.TryGetValue("jwt", out var token) &&
            !string.IsNullOrEmpty(token))
        {
            var userId = jwtService.ValidateToken(token);
            if (userId != null)
            {
                // Reconstruct the ClaimsPrincipal from the token claims.
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                _cachedState = new AuthenticationState(user);
                return;
            }
        }

        // No valid JWT — anonymous state
        _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_cachedState);
    }

    // Allows login/logout components to trigger UI updates immediately.
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}