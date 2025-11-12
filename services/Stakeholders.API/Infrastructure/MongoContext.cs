using MongoDB.Driver;
using Stakeholders.API.Domain.Models;

namespace Stakeholders.API.Infrastructure;

public class MongoContext
{
    public IMongoCollection<User> Users { get; }
    public MongoContext(IConfiguration cfg)
    {
        var client = new MongoClient(cfg["Mongo:Conn"]);
        var db = client.GetDatabase(cfg["Mongo:Db"]);
        Users = db.GetCollection<User>(cfg["Mongo:UsersCollection"]);
    }
}
