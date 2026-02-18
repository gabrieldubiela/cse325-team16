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

public class ReservationsPageTests : TestContext
{
    [Fact]
    public void AuthenticatedUser_SeesReservationsTable()
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

        // Mock de serviços
        var mockReservationService = new Mock<IReservationService>();
        mockReservationService
            .Setup(s => s.GetAsync())
            .ReturnsAsync(new List<Reservation>
            {
                new Reservation
                {
                    Id = "r1",
                    ClientId = "c1",
                    VehicleId = "v1",
                    Client = new Client { Name = "Alice" },
                    Vehicle = new Vehicle { Make = "Toyota", Model = "Corolla" },
                    StartDate = new DateTime(2025, 01, 01),
                    EndDate = new DateTime(2025, 01, 05),
                    TotalPrice = 400,
                    Status = ReservationStatus.Active
                },
                new Reservation
                {
                    Id = "r2",
                    ClientId = "c2",
                    VehicleId = "v2",
                    Client = new Client { Name = "Bob" },
                    Vehicle = new Vehicle { Make = "Honda", Model = "Civic" },
                    StartDate = new DateTime(2025, 02, 01),
                    EndDate = new DateTime(2025, 02, 03),
                    TotalPrice = 200,
                    Status = ReservationStatus.Completed
                }
            });

        // ClientService usado só para carregar clients na tela inicial
        var mockClientService = new Mock<IClientService>();
        mockClientService
            .Setup(s => s.GetAsync())
            .ReturnsAsync(new List<Client>
            {
                new Client { Id = "c1", Name = "Alice" },
                new Client { Id = "c2", Name = "Bob" }
            });

        // VehicleService pode ficar “vazio” nesse teste
        var mockVehicleService = new Mock<IVehicleService>();

        Services.AddSingleton<IReservationService>(mockReservationService.Object);
        Services.AddSingleton<IClientService>(mockClientService.Object);
        Services.AddSingleton<IVehicleService>(mockVehicleService.Object);

        // Act
        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Reservations>();

        // Assert: título, clientes, veículos e status aparecem
        Assert.Contains("Reservations", cut.Markup);
        Assert.Contains("Alice", cut.Markup);
        Assert.Contains("Toyota Corolla", cut.Markup);
        Assert.Contains("Bob", cut.Markup);
        Assert.Contains("Honda Civic", cut.Markup);
        Assert.Contains("Active", cut.Markup);
        Assert.Contains("Completed", cut.Markup);
    }
}
