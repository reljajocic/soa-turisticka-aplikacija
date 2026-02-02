using System.Security.Claims;
using Followers.API.Application.Interfaces;
using Followers.API.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Followers.API.API.Controllers;

[ApiController]
[Route("api/follow")] 
[Authorize]
public class FollowController : ControllerBase
{
    private readonly IFollowService _service;
    public FollowController(IFollowService service) => _service = service;

    [HttpPost("{followedId:guid}")]
    public async Task<IActionResult> Follow([FromRoute] Guid followedId, CancellationToken ct)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var res = await _service.FollowAsync(new FollowCommand(Guid.Parse(sub), followedId), ct);
        return res.Success ? Ok() : BadRequest();
    }

    [HttpDelete("{followedId:guid}")]
    public async Task<IActionResult> Unfollow([FromRoute] Guid followedId, CancellationToken ct)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var res = await _service.UnfollowAsync(new FollowCommand(Guid.Parse(sub), followedId), ct);
        return res.Success ? NoContent() : BadRequest();
    }

    [HttpGet("is-following")]
    public async Task<IActionResult> IsFollowing([FromQuery] Guid authorId, CancellationToken ct)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();

        var res = await _service.IsFollowingAsync(new FollowCheck(Guid.Parse(sub), authorId), ct);
        return Ok(res);
    }
}