using MongoDB.Bson; 
using MongoDB.Bson.Serialization.Attributes; 

namespace Tours.API.Domain.Models;

public class UserPosition
{
    [BsonId] // Ovo kaže: To je glavni ID
    [BsonRepresentation(BsonType.String)] // Ovo kaže: Čuvaj ga kao String, ne kao ObjectId
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    public double Lat { get; set; }
    public double Lon { get; set; }
    public DateTime UpdatedAt { get; set; }
}