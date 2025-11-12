namespace Tours.API.Domain.Models;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TourId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}