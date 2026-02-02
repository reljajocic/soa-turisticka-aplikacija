using MongoDB.Bson;
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

    public async Task<Blog> CreateAsync(Guid userId, string username, string avatarUrl, CreateBlogDto dto)
    {
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            Username = username,
            AuthorAvatarUrl = avatarUrl ?? "", 
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

    public async Task AddCommentAsync(Guid blogId, Guid userId, string username, string avatarUrl, string content)
    {
        var comment = new Comment
        {
            UserId = userId,
            Username = username,
            AuthorAvatarUrl = avatarUrl ?? "", 
            Content = content
        };

        var update = Builders<Blog>.Update.Push(b => b.Comments, comment);
        await _ctx.Blogs.UpdateOneAsync(b => b.Id == blogId, update);
    }

    public async Task UpdateAuthorAvatarAsync(Guid userId, string newAvatarUrl)
    {
        var updateBlog = Builders<Blog>.Update.Set(b => b.AuthorAvatarUrl, newAvatarUrl);
        await _ctx.Blogs.UpdateManyAsync(b => b.AuthorId == userId, updateBlog);

        var filter = Builders<Blog>.Filter.ElemMatch(b => b.Comments, c => c.UserId == userId);
        var updateComment = Builders<Blog>.Update.Set("Comments.$[elem].AuthorAvatarUrl", newAvatarUrl);
        var arrayFilters = new List<ArrayFilterDefinition>
    {
        new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.UserId", userId.ToString())) 
    };

        var blogsWithComments = await _ctx.Blogs.Find(b => b.Comments.Any(c => c.UserId == userId)).ToListAsync();
        foreach (var blog in blogsWithComments)
        {
            bool changed = false;
            foreach (var c in blog.Comments)
            {
                if (c.UserId == userId)
                {
                    c.AuthorAvatarUrl = newAvatarUrl;
                    changed = true;
                }
            }
            if (changed)
            {
                await _ctx.Blogs.ReplaceOneAsync(b => b.Id == blog.Id, blog);
            }
        }
    }
}