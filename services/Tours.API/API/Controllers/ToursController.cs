using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using Tours.API.Domain.Enums;

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
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var id = await _svc.CreateAsync(Guid.Parse(sub), dto);
        return Ok(new { id });
    }

    [HttpPost("{id:guid}/keypoints")]
    public async Task<IActionResult> AddKeypoint(Guid id, CreateKeypointDto dto)
    {
        var ok = await _svc.AddKeypointAsync(id, dto);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var tours = await _svc.GetAllAsync();
        return Ok(tours);
    }

    [HttpGet("author/{id:guid}")]
    public async Task<IActionResult> GetByAuthor(Guid id)
    {
        var tours = await _svc.GetByAuthorAsync(id);
        return Ok(tours);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await _svc.UpdateStatusAsync(id, (TourStatus)dto.Status, dto.Price);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var success = await _svc.DeleteAsync(id, Guid.Parse(sub));
        if (!success) return NotFound("Tour not found or access denied.");

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateTourDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var success = await _svc.UpdateAsync(id, Guid.Parse(sub), dto);
        if (!success) return NotFound();

        return Ok();
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {
        var tour = await _svc.GetAsync(id);
        if (tour is null) return NotFound();
        return Ok(tour);
    }

    [HttpGet("purchased")]
    public async Task<IActionResult> GetPurchased()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var tours = await _svc.GetPurchasedToursAsync(Guid.Parse(sub));
        return Ok(tours);
    }
}