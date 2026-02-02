using MongoDB.Driver;
using Tours.API.Domain.Models;
using Microsoft.Extensions.Configuration;

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

        Tours = db.GetCollection<Tour>("tours");
        Carts = db.GetCollection<ShoppingCart>("carts");
        Tokens = db.GetCollection<TourPurchaseToken>("tokens");
        Positions = db.GetCollection<UserPosition>("positions");
        Executions = db.GetCollection<TourExecution>("executions");
    }
}