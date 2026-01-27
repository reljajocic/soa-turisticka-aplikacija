using MongoDB.Driver;
using Tours.API.Domain.Models;

namespace Tours.API.Infrastructure;

public class TourMongoContext
{
    public IMongoCollection<Tour> Tours { get; }
    public IMongoCollection<ShoppingCart> Carts { get; } 
    public IMongoCollection<TourPurchaseToken> Tokens { get; }
    public IMongoCollection<UserPosition> Positions { get; }
    public IMongoCollection<TourExecution> Executions { get; }

    public TourMongoContext(IConfiguration cfg)
    {
        var client = new MongoClient(cfg["Mongo:Conn"]);
        var db = client.GetDatabase(cfg["Mongo:Db"]);

        db.DropCollection("positions");
        db.DropCollection("executions");

        Tours = db.GetCollection<Tour>("tours");
        Carts = db.GetCollection<ShoppingCart>("carts");
        Tokens = db.GetCollection<TourPurchaseToken>("tokens");
        Positions = db.GetCollection<UserPosition>("positions");
        Executions = db.GetCollection<TourExecution>("executions");
    }
}
// ❌ OVDE NE SME BITI "public static class Geo..." 
// (Jer je sada u Geo.cs)