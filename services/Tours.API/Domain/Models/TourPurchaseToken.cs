namespace Tours.API.Domain.Models;

public class TourPurchaseToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TourId { get; set; }
    public DateTime PurchaseDate { get; set; }
}