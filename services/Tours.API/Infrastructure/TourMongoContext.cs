using MongoDB.Driver;
using Tours.API.Domain.Models;

namespace Tours.API.Infrastructure;

public class TourMongoContext
{
    public IMongoCollection<Tour> Tours { get; }
    public IMongoCollection<UserPosition> Positions { get; }
    public IMongoCollection<CartItem> Cart { get; }
    public IMongoCollection<PurchaseToken> Purchases { get; }
    public IMongoCollection<TourExecution> Executions { get; }

    public TourMongoContext(IConfiguration cfg)
    {
        var client = new MongoClient(cfg["Mongo:Conn"]);
        var db = client.GetDatabase(cfg["Mongo:Db"]);
        Tours = db.GetCollection<Tour>("Tours");
        Positions = db.GetCollection<UserPosition>("Positions");
        Cart = db.GetCollection<CartItem>("Cart");
        Purchases = db.GetCollection<PurchaseToken>("Purchases");
        Executions = db.GetCollection<TourExecution>("Executions");
    }
}

public static class Geo
{
    public static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        lat1 *= Math.PI / 180.0; lat2 *= Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        double c = 2 * Math.Asin(Math.Sqrt(a));
        return R * c;
    }
}
