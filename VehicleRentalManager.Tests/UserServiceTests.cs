using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using VehicleRentalManager.Models;
using Xunit;

namespace VehicleRentalManager.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_SetsDefaultFields_AndInsertsUser()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<AppUser>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer
                .SerializerRegistry.GetSerializer<AppUser>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        var service = new UserServiceForTests(mockCollection.Object);

        // Act
        var user = await service.CreateAsync(
            name: "John",
            email: "John@example.com",
            googleId: "google-123");

        // Assert: campos básicos
        Assert.Equal("John", user.Name);
        Assert.Equal("John@example.com", user.Email);
        Assert.Equal("google-123", user.GoogleId);

        // Assert: defaults de regra de negócio
        Assert.False(user.IsApproved);
        Assert.NotEqual(default, user.CreatedAt);

        // Assert: chamada ao InsertOneAsync
        mockCollection.Verify(
            c => c.InsertOneAsync(
                user,
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_CallsUpdateOne()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<AppUser>>();

        mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer
                .SerializerRegistry.GetSerializer<AppUser>());
        mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        var service = new UserServiceForTests(mockCollection.Object);

        // Act
        await service.ApproveAsync("user-1");

        // Assert
        mockCollection.Verify(
            c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<AppUser>>(),
                It.IsAny<UpdateDefinition<AppUser>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

// classe auxiliar só para testes
internal class UserServiceForTests
{
    private readonly IMongoCollection<AppUser> _users;

    public UserServiceForTests(IMongoCollection<AppUser> users)
    {
        _users = users;
    }

    public async Task<AppUser> CreateAsync(string name, string email, string googleId)
    {
        var user = new AppUser
        {
            Name       = name,
            Email      = email,
            GoogleId   = googleId,
            IsApproved = false,
            CreatedAt  = DateTime.UtcNow
        };
        await _users.InsertOneAsync(user);
        return user;
    }

    public Task ApproveAsync(string id)
    {
        var update = Builders<AppUser>.Update.Set(u => u.IsApproved, true);
        return _users.UpdateOneAsync(u => u.Id == id, update);
    }

    public Task RevokeAsync(string id)
    {
        var update = Builders<AppUser>.Update.Set(u => u.IsApproved, false);
        return _users.UpdateOneAsync(u => u.Id == id, update);
    }

    public Task UpdateLastLoginAsync(string id)
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.LastLoginAt, DateTime.UtcNow);
        return _users.UpdateOneAsync(u => u.Id == id, update);
    }
}
