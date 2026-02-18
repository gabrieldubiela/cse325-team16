using System;
using System.Collections.Generic;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class VehiclesPageTests : TestContext
{
    [Fact]
    public void AuthenticatedUser_SeesVehiclesTable()
    {
        // Auth: usuário autenticado via bUnit
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        // AuthService (bUnit já registrou AuthenticationStateProvider)
        Services.AddSingleton<AuthService>();

        // Mock VehicleService
        var mockVehicleService = new Mock<IVehicleService>();
        mockVehicleService
            .Setup(s => s.GetAsync())
            .ReturnsAsync(new List<Vehicle>
            {
                new Vehicle
                {
                    Id = "v1",
                    Make = "Toyota",
                    Model = "Corolla",
                    Color = "White",
                    Year = 2020,
                    LicensePlate = "ABC-1234",
                    PricePerDay = 100,
                    IsAvailable = true
                },
                new Vehicle
                {
                    Id = "v2",
                    Make = "Honda",
                    Model = "Civic",
                    Color = "Black",
                    Year = 2019,
                    LicensePlate = "XYZ-5678",
                    PricePerDay = 90,
                    IsAvailable = false
                }
            });

        Services.AddSingleton<IVehicleService>(mockVehicleService.Object);

        // IConfiguration fake para o MongoDbService
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDb:DatabaseName"] = "test-db"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        Services.AddSingleton(configuration);

        // Registrar MongoDbService e UserService reais (não serão usados de fato no teste)
        Services.AddSingleton<MongoDbService>();
        Services.AddSingleton<UserService>();

        // Act
        var cut = RenderComponent<VehicleRentalManager.Components.Pages.Vehicles>();

        // Assert: título, veículos e badges aparecem
        Assert.Contains("Vehicles", cut.Markup);
        Assert.Contains("Toyota", cut.Markup);
        Assert.Contains("Corolla", cut.Markup);
        Assert.Contains("ABC-1234", cut.Markup);
        Assert.Contains("Honda", cut.Markup);
        Assert.Contains("Civic", cut.Markup);
        Assert.Contains("XYZ-5678", cut.Markup);
        Assert.Contains("Available", cut.Markup);
        Assert.Contains("Unavailable", cut.Markup); // texto do badge na página
    }
}
