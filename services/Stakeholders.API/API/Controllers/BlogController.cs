using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stakeholders.API.Application.Interfaces;
using Stakeholders.API.DTOs;
using System.Security.Claims;

namespace Stakeholders.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _service;

    public BlogController(IBlogService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateBlogDto dto)
    {
        // Tražimo ID (sub)
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // POPRAVKA: Prvo tražimo "username" jer ga tako zovemo u AuthService.cs
        var username = User.FindFirst("username")?.Value
                       ?? User.FindFirst("preferred_username")?.Value
                       ?? User.FindFirst(ClaimTypes.Name)?.Value
                       ?? "Unknown User";

        var blog = await _service.CreateAsync(userId, username, dto);
        return Ok(blog);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var blogs = await _service.GetAllAsync();
        return Ok(blogs);
    }

    // Endpoint za jedan blog (Detalji)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var blog = await _service.GetAsync(id);
        if (blog == null) return NotFound();
        return Ok(blog);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBlogs()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var blogs = await _service.GetMyBlogsAsync(userId);
        return Ok(blogs);
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid id, CreateCommentDto dto)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // ISTA POPRAVKA I ZA KOMENTARE
        var username = User.FindFirst("username")?.Value
                       ?? User.FindFirst("preferred_username")?.Value
                       ?? "Unknown User";

        await _service.AddCommentAsync(id, userId, username, dto.Content);
        return Ok();
    }
}