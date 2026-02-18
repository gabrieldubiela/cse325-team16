using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace VehicleRentalManager.Tests;

public class TestAuthStateProvider : AuthenticationStateProvider
{
    private readonly bool _isAuthenticated;
    private readonly string? _userName;

    public TestAuthStateProvider(bool isAuthenticated, string? userName = null)
    {
        _isAuthenticated = isAuthenticated;
        _userName = userName;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsIdentity identity;

        if (_isAuthenticated)
        {
            identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, _userName ?? "User")
            }, "TestAuth");
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}
