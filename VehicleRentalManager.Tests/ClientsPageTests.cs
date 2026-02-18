using System.Collections.Generic;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class ClientsPageTests : TestContext
{
    [Fact]
    public void AuthenticatedUser_SeesClientsTable()
    {
        // Auth fake
        var authStateProvider = new TestAuthStateProvider(
            isAuthenticated: true,
            userName: "Manager");

        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorization();
        Services.AddCascadingAuthenticationState();

        // Mock do ClientService / IClientService
        var mockClientService = new Mock<IClientService>();
        mockClientService
            .Setup(s => s.GetAsync())
            .ReturnsAsync(new List<Client>
            {
                new Client { Id = "1", Name = "Alice", Email = "alice@test.com", Phone = "123", DriverLicense = "DL1", Address = "Street 1", IsActive = true },
                new Client { Id = "2", Name = "Bob",   Email = "bob@test.com",   Phone = "456", DriverLicense = "DL2", Address = "Street 2", IsActive = false },
            });

        Services.AddSingleton<IClientService>(mockClientService.Object);

        // Act
        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Clients>();

        // Assert: título, tabela e nomes
        Assert.Contains("Clients", cut.Markup);
        Assert.Contains("Alice", cut.Markup);
        Assert.Contains("Bob", cut.Markup);
        Assert.Contains("Active", cut.Markup);
        Assert.Contains("Inactive", cut.Markup);
    }

    [Fact]
    public void ClickCreateNewClient_ShowsForm()
    {
        var authStateProvider = new TestAuthStateProvider(
            isAuthenticated: true,
            userName: "Manager");

        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorization();
        Services.AddCascadingAuthenticationState();

        var mockClientService = new Mock<IClientService>();
        mockClientService
            .Setup(s => s.GetAsync())
            .ReturnsAsync(new List<Client>()); // sem clientes para esse teste

        Services.AddSingleton<IClientService>(mockClientService.Object);

        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Clients>();

        // botão "Create New Client"
        var createButton = cut.Find("button.btn.btn-primary");
        createButton.Click();

        // depois do clique, o formulário deve aparecer
        Assert.Contains("Name", cut.Markup);
        Assert.Contains("Email", cut.Markup);
        Assert.Contains("Driver License", cut.Markup);
        Assert.Contains("Save", cut.Markup);
    }
}
