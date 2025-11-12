using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;

namespace Tours.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ToursController : ControllerBase
{
    private readonly ITourService _svc;
    public ToursController(ITourService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTourDto dto)
    {
        var sub = User.FindFirst("sub")?.Value; if (sub is null) return Unauthorized();
        var id = await _svc.CreateAsync(Guid.Parse(sub), dto);
        return Ok(new { id });
    }

    [HttpPost("{id:guid}/keypoints")]
    public async Task<IActionResult> AddKeypoint(Guid id, CreateKeypointDto dto)
    {
        var ok = await _svc.AddKeypointAsync(id, dto);
        return ok ? NoContent() : NotFound();
    }
}