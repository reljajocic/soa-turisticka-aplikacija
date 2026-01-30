using MongoDB.Driver;
using Stakeholders.API.Domain.Models;

namespace Stakeholders.API.Infrastructure;

public class MongoContext
{
    // 1. Ovde definišemo property-je (samo get;)
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Blog> Blogs { get; }

    public MongoContext(IConfiguration cfg)
    {
        var client = new MongoClient(cfg["Mongo:Conn"]);
        var db = client.GetDatabase(cfg["Mongo:Db"]);

        // 2. Ovde im dodeljujemo vrednost koristeći 'db' varijablu
        Users = db.GetCollection<User>(cfg["Mongo:UsersCollection"]);
        Blogs = db.GetCollection<Blog>("blogs");
    }
}