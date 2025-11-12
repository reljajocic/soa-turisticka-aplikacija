using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;

namespace Tours.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _svc;
    public CartController(ICartService svc) => _svc = svc;

    [HttpPost("items")]
    public async Task<IActionResult> Add(AddCartDto dto)
    {
        var sub = User.FindFirst("sub")?.Value; if (sub is null) return Unauthorized();
        var id = await _svc.AddItemAsync(Guid.Parse(sub), dto);
        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var sub = User.FindFirst("sub")?.Value; if (sub is null) return Unauthorized();
        var (items, total) = await _svc.GetAsync(Guid.Parse(sub));
        return Ok(new { items, total });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var sub = User.FindFirst("sub")?.Value; if (sub is null) return Unauthorized();
        var tokens = await _svc.CheckoutAsync(Guid.Parse(sub));
        return Ok(tokens);
    }
}