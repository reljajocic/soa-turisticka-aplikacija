using MongoDB.Driver;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;
using Tours.API.Infrastructure;

namespace Tours.API.Application.Services;

public class CartService : ICartService
{
    private readonly TourMongoContext _ctx;
    public CartService(TourMongoContext ctx) => _ctx = ctx;

    public async Task<Guid> AddItemAsync(Guid userId, AddCartDto dto)
    {
        var i = new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TourId = dto.TourId,
            Name = dto.Name,
            // FIX: Dodato (decimal) jer je dto.Price double
            Price = (decimal)dto.Price
        };

        await _ctx.Cart.InsertOneAsync(i);
        return i.Id;
    }

    public async Task<(IReadOnlyList<CartItem> items, decimal total)> GetAsync(Guid userId)
    {
        var items = await _ctx.Cart.Find(x => x.UserId == userId).ToListAsync();
        return (items, items.Sum(x => x.Price));
    }

    public async Task<IReadOnlyList<PurchaseToken>> CheckoutAsync(Guid userId)
    {
        var items = await _ctx.Cart.Find(x => x.UserId == userId).ToListAsync();
        var tokens = items.Select(i => new PurchaseToken { Id = Guid.NewGuid(), UserId = userId, TourId = i.TourId, CreatedAt = DateTime.UtcNow }).ToList();
        if (tokens.Any()) await _ctx.Purchases.InsertManyAsync(tokens);
        await _ctx.Cart.DeleteManyAsync(x => x.UserId == userId);
        return tokens;
    }
}