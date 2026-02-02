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
    private readonly IProfileService _profileService;
    private readonly IBlogService _blogService;
    public ProfileController(IProfileService profileService, IBlogService blogService)
    {
        _profileService = profileService;
        _blogService = blogService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var u = await _profileService.GetMeAsync(Guid.Parse(sub));
        return u is null ? NotFound() : Ok(u);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var u = await _profileService.GetProfileAsync(id);
        if (u is null) return NotFound();

        return Ok(u);
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var userId = Guid.Parse(sub);

        // 1. Ažuriraj Profil
        var ok = await _profileService.UpdateProfileAsync(userId, dto.FirstName, dto.LastName, dto.Bio, dto.Motto, dto.AvatarUrl);

        if (ok)
        {
            // 2. AKO je uspešno, ažuriraj i sve blogove/komentare!
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                await _blogService.UpdateAuthorAvatarAsync(userId, dto.AvatarUrl);
            }
            return NoContent();
        }

        return NotFound();
    }

    public record UpdateDto(string? FirstName, string? LastName, string? Bio, string? Motto, string? AvatarUrl);
}
