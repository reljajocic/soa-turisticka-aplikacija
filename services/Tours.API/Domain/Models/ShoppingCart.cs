namespace Tours.API.Domain.Models;

public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    public double CalculateTotal() => Items.Sum(i => i.Price);
}

public class OrderItem
{
    public Guid TourId { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}