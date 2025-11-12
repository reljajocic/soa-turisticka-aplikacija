using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;

namespace Tours.API.Application.Interfaces;

public interface ICartService
{
    Task<Guid> AddItemAsync(Guid userId, AddCartDto dto);
    Task<(IReadOnlyList<CartItem> items, decimal total)> GetAsync(Guid userId);
    Task<IReadOnlyList<PurchaseToken>> CheckoutAsync(Guid userId);
}