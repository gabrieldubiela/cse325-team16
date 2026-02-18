using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using VehicleRentalManager.Models;
using VehicleRentalManager.Services;
using Xunit;

namespace VehicleRentalManager.Tests;

public class VehicleServiceTests
{
    [Fact]
    public async Task CreateAsync_InsertsVehicleInCollection()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Vehicle>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer
                .SerializerRegistry.GetSerializer<Vehicle>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        // vamos injetar a collection direto no serviço
        var service = new VehicleServiceForTests(mockCollection.Object);

        var vehicle = new Vehicle
        {
            Id = "veh-1",
            Model = "Carro Teste",
            IsAvailable = true
        };

        // Act
        await service.CreateAsync(vehicle);

        // Assert: InsertOneAsync chamado com o veículo
        mockCollection.Verify(
            c => c.InsertOneAsync(
                vehicle,
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task GetByIdAsync_ReturnsVehicleFromCollection()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Vehicle>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer
                .SerializerRegistry.GetSerializer<Vehicle>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        var expectedVehicle = new Vehicle
        {
            Id = "veh-1",
            Model = "Carro Teste"
        };

        // cursor que retorna uma única iteração com o veículo
        var mockCursor = new Mock<IAsyncCursor<Vehicle>>();
        mockCursor.Setup(_ => _.Current).Returns(new[] { expectedVehicle });

        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)   // primeira chamada: tem dados
            .Returns(false); // segunda chamada: acabou

        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Vehicle>>(),
                It.IsAny<FindOptions<Vehicle, Vehicle>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var service = new VehicleServiceForTests(mockCollection.Object);

        // Act
        var result = await service.GetByIdAsync("veh-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("veh-1", result!.Id);
        Assert.Equal("Carro Teste", result.Model);
    }

    // Classe auxiliar só para testes, para injetar a collection direto
    internal class VehicleServiceForTests : IVehicleService
    {
        private readonly IMongoCollection<Vehicle> _vehicles;

        public VehicleServiceForTests(IMongoCollection<Vehicle> vehicles)
        {
            _vehicles = vehicles;
        }

        public Task<List<Vehicle>> GetAllAsync()
            => _vehicles.Find(_ => true).ToListAsync();

        public Task<List<Vehicle>> GetAsync()
            => _vehicles.Find(_ => true).ToListAsync();

        public Task CreateAsync(Vehicle vehicle)
            => _vehicles.InsertOneAsync(vehicle);

        public Task DeleteAsync(string id)
            => _vehicles.DeleteOneAsync(v => v.Id == id);

        public Task<Vehicle> GetByIdAsync(string id)
            => _vehicles.Find(v => v.Id == id).FirstOrDefaultAsync()!;

        public Task UpdateAsync(string id, Vehicle vehicle)
            => _vehicles.ReplaceOneAsync(v => v.Id == id, vehicle);
    }
}

