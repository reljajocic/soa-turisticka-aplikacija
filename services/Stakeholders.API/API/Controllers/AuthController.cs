using Microsoft.AspNetCore.Mvc;
using Stakeholders.API.Application.Interfaces;
using System.Security.Claims;

namespace Stakeholders.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var u = await _auth.RegisterAsync(dto.Username, dto.Email, dto.Password, dto.Role);
        return u is null ? Conflict("User exists") : CreatedAtAction(nameof(Register), new { u.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _auth.LoginAsync(dto.Username, dto.Password);
        return token is null ? Unauthorized() : Ok(new { token });
    }

    public record RegisterDto(string Username, string Email, string Password, string Role);
    public record LoginDto(string Username, string Password);
}
