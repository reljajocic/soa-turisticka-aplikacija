using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using System.Security.Claims;

namespace Tours.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _svc;
    public CartController(ICartService svc) => _svc = svc;

    // ISPRAVLJENO: Nema više ("items"), sada je čist HttpPost -> /api/cart
    [HttpPost]
    public async Task<IActionResult> AddToCart(AddCartDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        // ISPRAVLJENO: Pozivamo AddToCartAsync (kako se zove u servisu)
        await _svc.AddToCartAsync(Guid.Parse(sub), dto);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var cart = await _svc.GetCartAsync(Guid.Parse(sub));

        // Vraćamo format koji frontend očekuje
        return Ok(new { items = cart.Items, total = cart.CalculateTotal() });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var tokens = await _svc.CheckoutAsync(Guid.Parse(sub));
        return Ok(tokens);
    }
}