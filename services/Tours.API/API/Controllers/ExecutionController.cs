using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using System.Security.Claims;

namespace Tours.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExecutionController : ControllerBase
{
    private readonly IExecutionService _svc;
    public ExecutionController(IExecutionService svc) => _svc = svc;

    [HttpPost("start")]
    public async Task<IActionResult> Start(StartExecDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var id = await _svc.StartAsync(Guid.Parse(sub), dto);
        return Ok(new { id });
    }

    [HttpPost("poll")]
    public async Task<IActionResult> Poll(PollDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        var res = await _svc.PollAsync(Guid.Parse(sub), dto);
        return Ok(res);
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish(FinishDto dto)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub is null) return Unauthorized();
        await _svc.FinishAsync(Guid.Parse(sub), dto);
        return NoContent();
    }
}