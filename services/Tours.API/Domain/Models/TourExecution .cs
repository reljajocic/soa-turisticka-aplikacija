using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tours.API.Domain.Models;

public class TourExecution
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid TourId { get; set; }

    public DateTime StartedAt { get; set; }
    public string Status { get; set; } = "Active";
    public int ProgressIndex { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime? FinishedAt { get; set; }
}