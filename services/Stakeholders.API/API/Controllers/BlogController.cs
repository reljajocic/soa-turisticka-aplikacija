using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stakeholders.API.Application.Interfaces;
using Stakeholders.API.Application.Services;
using Stakeholders.API.DTOs;
using System.Security.Claims;

namespace Stakeholders.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly IProfileService _profileService;

    public BlogController(IBlogService blogService, IProfileService profileService)
    {
        _blogService = blogService;
        _profileService = profileService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateBlogDto dto)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var username = User.FindFirst("preferred_username")?.Value
                       ?? User.FindFirst("unique_name")?.Value
                       ?? "Unknown User";

        string avatarUrl = "";

        try
        {
            var profile = await _profileService.GetMeAsync(userId);
            if (profile != null)
            {
                avatarUrl = profile.AvatarUrl;
                if (!string.IsNullOrEmpty(profile.Username))
                {
                    username = profile.Username;
                }
            }
        }
        catch { }

        var blog = await _blogService.CreateAsync(userId, username, avatarUrl, dto);
        return Ok(blog);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var blogs = await _blogService.GetAllAsync();
        return Ok(blogs);
    }

    // Endpoint za jedan blog (Detalji)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var blog = await _blogService.GetAsync(id);
        if (blog == null) return NotFound();
        return Ok(blog);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBlogs()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var blogs = await _blogService.GetMyBlogsAsync(userId);
        return Ok(blogs);
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid id, CreateCommentDto dto)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var username = User.FindFirst("username")?.Value
                       ?? User.FindFirst("preferred_username")?.Value
                       ?? User.FindFirst(ClaimTypes.Name)?.Value
                       ?? "Unknown User";

        string avatarUrl = "";

        try
        {
            var profile = await _profileService.GetMeAsync(userId);
            if (profile != null)
            {
                avatarUrl = profile.AvatarUrl;
                if (!string.IsNullOrEmpty(profile.Username))
                {
                    username = profile.Username;
                }
            }
        }
        catch { }

        await _blogService.AddCommentAsync(id, userId, username, avatarUrl, dto.Content);
        return Ok();
    }
}