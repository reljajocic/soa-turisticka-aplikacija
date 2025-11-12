namespace Tours.API.Domain.Models;

public class PurchaseToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TourId { get; set; }
    public DateTime CreatedAt { get; set; }
}