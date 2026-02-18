using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace VehicleRentalManager.Tests;

public class FakeAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _user;

    public FakeAuthenticationStateProvider(ClaimsPrincipal user)
    {
        _user = user;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = new AuthenticationState(_user);
        return Task.FromResult(state);
    }
}
