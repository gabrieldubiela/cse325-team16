using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Moq;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task IsVehicleAvailableAsync_WhenNoConflictingReservation_ReturnsTrue()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Reservation>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer
                .SerializerRegistry.GetSerializer<Reservation>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        // cursor vazio (nenhuma reserva conflitante)
        var mockCursor = new Mock<IAsyncCursor<Reservation>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

        mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Reservation>>(),
                It.IsAny<FindOptions<Reservation, Reservation>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var mockClientService = new Mock<IClientService>();
        var mockVehicleService = new Mock<IVehicleService>();

        var service = new ReservationService(
            mockCollection.Object,
            mockClientService.Object,
            mockVehicleService.Object);

        var vehicleId = "veh-1";
        var start = new DateTime(2026, 02, 20);
        var end = new DateTime(2026, 02, 22);

        // Act
        var available = await service.IsVehicleAvailableAsync(vehicleId, start, end);

        // Assert
        Assert.True(available);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAt_AndCallsClientUpdate()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Reservation>>();
        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer
                .SerializerRegistry.GetSerializer<Reservation>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        var mockClientService = new Mock<IClientService>();
        var mockVehicleService = new Mock<IVehicleService>();

        var service = new ReservationService(
            mockCollection.Object,
            mockClientService.Object,
            mockVehicleService.Object);

        var reservation = new Reservation
        {
            ClientId = "client-1",
            VehicleId = "veh-1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await service.CreateAsync(reservation);

        // Assert: CreatedAt foi setado
        Assert.NotEqual(default, reservation.CreatedAt);

        // Assert: InsertOneAsync chamado
        mockCollection.Verify(
            c => c.InsertOneAsync(
                reservation,
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Assert: UpdateLastRentalDateAsync do ClientService chamado
        mockClientService.Verify(
            cs => cs.UpdateLastRentalDateAsync("client-1"),
            Times.Once);
    }
}