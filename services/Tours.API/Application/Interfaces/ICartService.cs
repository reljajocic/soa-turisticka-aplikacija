using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;

namespace Tours.API.Application.Interfaces;

public interface ICartService
{
    Task<ShoppingCart> GetCartAsync(Guid userId);
    Task AddToCartAsync(Guid userId, AddCartDto item);
    Task<List<TourPurchaseToken>> CheckoutAsync(Guid userId);
    Task RemoveFromCartAsync(Guid userId, Guid tourId);
}