using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class HomePageTests : TestContext
{
    [Fact]
    public void AuthenticatedUser_SeesWelcomeWithName()
    {
        var authStateProvider = new TestAuthStateProvider(
            isAuthenticated: true,
            userName: "Gabriel");

        // Authentication
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddSingleton<AuthService>();

        // Authorization: registra nosso fake explicitamente
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorization();              // registra políticas básicas
        Services.AddCascadingAuthenticationState();

        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Home>();

        Assert.Contains("Welcome, Gabriel!", cut.Markup);
        Assert.Contains("You are signed in to Vehicle Rental Manager.", cut.Markup);
    }
}
