using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using System.Security.Claims;

namespace Tours.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionController : ControllerBase
{
    private readonly IPositionService _svc;
    public PositionController(IPositionService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Set(SetPosDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        await _svc.SetAsync(Guid.Parse(sub), dto); return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var p = await _svc.GetAsync(Guid.Parse(sub)); return p is null ? NotFound() : Ok(p);
    }
}