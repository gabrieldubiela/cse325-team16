using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class ClientServiceTests
{
    [Fact]
    public async Task CreateAsync_SetsCreatedAt_AndInsertsClient()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Client>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry.GetSerializer<Client>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        var service = new ClientService(mockCollection.Object);

        var client = new Client
        {
            Name  = "João",
            Email = "joao@example.com"
        };

        // Act
        await service.CreateAsync(client);

        // Assert
        Assert.NotEqual(default, client.CreatedAt);

        mockCollection.Verify(
            c => c.InsertOneAsync(
                client,
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateLastRentalDateAsync_UpdatesLastRentalDateField()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Client>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry.GetSerializer<Client>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        var service = new ClientService(mockCollection.Object);
        var clientId = "123";

        // Act
        await service.UpdateLastRentalDateAsync(clientId);

        // Assert
        mockCollection.Verify(
            c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Client>>(),
                It.IsAny<UpdateDefinition<Client>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
