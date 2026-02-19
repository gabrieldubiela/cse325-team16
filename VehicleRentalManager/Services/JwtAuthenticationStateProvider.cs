using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace VehicleRentalManager.Services;

// Blazor Server renders components twice:
//   1. Static prerender (HttpContext IS available)
//   2. Interactive SignalR circuit (HttpContext is NULL)
// PersistentComponentState so the circuit can rehydrate without HttpContext.
public class JwtAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PersistentComponentState _persistentState;
    private readonly PersistingComponentStateSubscription _subscription;

    private Task<AuthenticationState>? _cachedTask;

    private record UserInfo(string UserId, string Email, string Name);

    public JwtAuthenticationStateProvider(
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor,
        PersistentComponentState persistentState)
    {
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _persistentState = persistentState;

        // RegisterOnPersisting runs just before the prerender HTML is flushed.
        // We use it to embed the authenticated user into the page so the circuit can read it.
        _subscription = persistentState.RegisterOnPersisting(PersistAuthState);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Cache so we don't rebuild on every call within the same circuit lifetime.
        _cachedTask ??= BuildAuthStateAsync();
        return _cachedTask;
    }

    private Task<AuthenticationState> BuildAuthStateAsync()
    {
        // ── Path 1: SignalR circuit — rehydrate from state persisted during prerender ──
        if (_persistentState.TryTakeFromJson<UserInfo>("JwtUser", out var userInfo) &&
            userInfo is not null)
        {
            var identity = BuildIdentity(userInfo.UserId, userInfo.Email, userInfo.Name);
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        // ── Path 2: Prerender — HttpContext is available, read the JWT cookie ──
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null &&
            httpContext.Request.Cookies.TryGetValue("jwt", out var token) &&
            !string.IsNullOrEmpty(token))
        {
            var userId = _jwtService.ValidateToken(token);
            if (userId is not null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var email = jwt.Claims
                    .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? "";
                var name = jwt.Claims
                    .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value ?? "";

                var identity = BuildIdentity(userId, email, name);
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
            }
        }

        // ── Anonymous ──
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    private Task PersistAuthState()
    {
        // Runs during prerender. If the user is authenticated, embed their info
        // into the HTML so the circuit can rehydrate without needing HttpContext.
        var authState = GetAuthenticationStateAsync().Result;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst("userId")?.Value
                      ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var email  = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                      ?? user.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var name   = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                      ?? user.FindFirst(ClaimTypes.Name)?.Value ?? "";

            _persistentState.PersistAsJson("JwtUser", new UserInfo(userId, email, name));
        }

        return Task.CompletedTask;
    }

    private static ClaimsIdentity BuildIdentity(string userId, string email, string name)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,      userId),
            new Claim("userId",                       userId),
            new Claim(ClaimTypes.Email,               email),
            new Claim(JwtRegisteredClaimNames.Email,  email),
            new Claim(ClaimTypes.Name,                name),
            new Claim(JwtRegisteredClaimNames.Name,   name),
        };
        return new ClaimsIdentity(claims, "jwt");
    }

    public void Dispose() => _subscription.Dispose();
}