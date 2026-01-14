using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stakeholders.API.Application.Interfaces;
using System.Security.Claims;

namespace Stakeholders.API.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _service;
    public ProfileController(IProfileService service) => _service = service;

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var u = await _service.GetMeAsync(Guid.Parse(sub));
        return u is null ? NotFound() : Ok(u);
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var ok = await _service.UpdateProfileAsync(Guid.Parse(sub), dto.FirstName, dto.LastName, dto.Bio, dto.Motto, dto.AvatarUrl);
        return ok ? NoContent() : NotFound();
    }

    public record UpdateDto(string? FirstName, string? LastName, string? Bio, string? Motto, string? AvatarUrl);
}
