using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using VehicleRentalManager.Services;

namespace VehicleRentalManager.Services;

// Bridges the gap between ASP.NET Core's HTTP context (cookies) and Blazor's SignalR circuit.
// We manually extract the JWT from the cookie because standard Identity state doesn't automatically flow to the Blazor circuit.
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;

    // Inject IHttpContextAccessor to access the initial HTTP request cookies during the SignalR circuit establishment.
    public JwtAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Attempt to retrieve the JWT from the cookie to re-hydrate the user's identity within the Blazor circuit.
        if (httpContext != null &&
            httpContext.Request.Cookies.TryGetValue("jwt", out var token) &&
            !string.IsNullOrEmpty(token))
        {
            var userId = _jwtService.ValidateToken(token);
            if (userId != null)
            {
                // Reconstruct the ClaimsPrincipal manually from the token because the standard
                // ASP.NET Core authentication middleware pipeline doesn't run for SignalR messages.
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            }
        }

        // No valid JWT — return anonymous
        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    // Expose state change notification publicly to allow login/logout components to trigger UI updates immediately.
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}