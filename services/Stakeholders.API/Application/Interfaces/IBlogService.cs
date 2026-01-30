using Stakeholders.API.Domain.Models;
using Stakeholders.API.DTOs;

namespace Stakeholders.API.Application.Interfaces;

public interface IBlogService
{
    Task<Blog> CreateAsync(Guid userId, string username, CreateBlogDto dto);
    Task<List<Blog>> GetAllAsync(); // Feed
    Task<List<Blog>> GetMyBlogsAsync(Guid userId);
    Task<Blog?> GetAsync(Guid id);
    Task AddCommentAsync(Guid blogId, Guid userId, string username, string content);
}