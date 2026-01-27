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

    public async Task<ShoppingCart> GetCartAsync(Guid userId)
    {
        var cart = await _ctx.Carts.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        if (cart == null)
        {
            cart = new ShoppingCart { Id = Guid.NewGuid(), UserId = userId, Items = new List<OrderItem>() };
            await _ctx.Carts.InsertOneAsync(cart);
        }
        return cart;
    }

    public async Task AddToCartAsync(Guid userId, AddCartDto item)
    {
        var cart = await GetCartAsync(userId);

        // Provera da li već postoji u korpi
        if (!cart.Items.Any(i => i.TourId == item.TourId))
        {
            var newItem = new OrderItem
            {
                TourId = item.TourId,
                Name = item.Name,
                Price = item.Price
            };

            var update = Builders<ShoppingCart>.Update.Push(x => x.Items, newItem);
            await _ctx.Carts.UpdateOneAsync(x => x.UserId == userId, update);
        }
    }

    public async Task<List<TourPurchaseToken>> CheckoutAsync(Guid userId)
    {
        var cart = await GetCartAsync(userId);
        if (cart.Items.Count == 0) return new List<TourPurchaseToken>();

        var tokens = new List<TourPurchaseToken>();

        foreach (var item in cart.Items)
        {
            var token = new TourPurchaseToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TourId = item.TourId,
                PurchaseDate = DateTime.UtcNow
            };
            tokens.Add(token);
        }

        if (tokens.Count > 0)
        {
            await _ctx.Tokens.InsertManyAsync(tokens);

            // Isprazni korpu nakon kupovine
            var update = Builders<ShoppingCart>.Update.Set(x => x.Items, new List<OrderItem>());
            await _ctx.Carts.UpdateOneAsync(x => x.UserId == userId, update);
        }

        return tokens;
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid tourId)
    {
        // MongoDB magija: "Izbaci iz niza Items onaj element čiji je TourId == tourId"
        var update = Builders<ShoppingCart>.Update.PullFilter(x => x.Items, i => i.TourId == tourId);

        await _ctx.Carts.UpdateOneAsync(x => x.UserId == userId, update);
    }
}