using MongoDB.Driver;
using Stakeholders.API.Application.Interfaces;
using Stakeholders.API.Domain.Models;
using Stakeholders.API.DTOs;
using Stakeholders.API.Infrastructure;

namespace Stakeholders.API.Application.Services;

public class BlogService : IBlogService
{
    private readonly MongoContext _ctx;

    public BlogService(MongoContext ctx) => _ctx = ctx;

    public async Task<Blog> CreateAsync(Guid userId, string username, CreateBlogDto dto)
    {
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            Username = username,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrls = dto.ImageUrls ?? new List<string>(),
            CreatedAt = DateTime.UtcNow
        };

        await _ctx.Blogs.InsertOneAsync(blog);
        return blog;
    }

    public async Task<List<Blog>> GetAllAsync()
    {
        // Sortiramo od najnovijeg ka najstarijem
        return await _ctx.Blogs.Find(_ => true)
            .SortByDescending(b => b.CreatedAt)
            .Limit(50)
            .ToListAsync();
    }

    public async Task<List<Blog>> GetMyBlogsAsync(Guid userId)
    {
        return await _ctx.Blogs.Find(b => b.AuthorId == userId)
            .SortByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Blog?> GetAsync(Guid id)
    {
        return await _ctx.Blogs.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddCommentAsync(Guid blogId, Guid userId, string username, string content)
    {
        var comment = new Comment
        {
            UserId = userId,
            Username = username,
            Content = content
        };

        // MongoDB atomic update: Dodajemo komentar u niz unutar bloga
        var update = Builders<Blog>.Update.Push(b => b.Comments, comment);
        await _ctx.Blogs.UpdateOneAsync(b => b.Id == blogId, update);
    }
}