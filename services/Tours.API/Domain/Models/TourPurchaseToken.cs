using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tours.API.Domain.Models;

public class TourPurchaseToken
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid TourId { get; set; }

    public DateTime PurchaseDate { get; set; }
}