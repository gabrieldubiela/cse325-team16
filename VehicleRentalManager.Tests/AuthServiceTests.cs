using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task GetCurrentUserAsync_WhenUserIsAuthenticated_ReturnsCorrectDto()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, "João da Silva"),
            new Claim(JwtRegisteredClaimNames.Email, "joao@example.com")
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var user     = new ClaimsPrincipal(identity);

        AuthenticationStateProvider fakeProvider = new FakeAuthenticationStateProvider(user);
        var authService = new AuthService(fakeProvider);

        // Act
        var result = await authService.GetCurrentUserAsync();

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal("João da Silva", result.Name);
        Assert.Equal("joao@example.com", result.Email);
    }

        [Fact]
    public async Task GetCurrentUserAsync_WhenUserIsNotAuthenticated_ReturnsAnonymousDto()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // sem authenticationType -> não autenticado
        var user     = new ClaimsPrincipal(identity);

        AuthenticationStateProvider fakeProvider = new FakeAuthenticationStateProvider(user);
        var authService = new AuthService(fakeProvider);

        // Act
        var result = await authService.GetCurrentUserAsync();

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.Name);
        Assert.Null(result.Email);
    }

}
