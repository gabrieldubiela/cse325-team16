using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

// Provides a scoped access point to the database, useful for dependency injection patterns
// where a specific database instance is required rather than the raw client.
public class MongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(IConfiguration config)
    {
        var connectionString = config["MongoDb:ConnectionString"];
        var dbName = config["MongoDb:DatabaseName"];

        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(dbName);
    }
}
