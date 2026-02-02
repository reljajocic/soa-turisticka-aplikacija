namespace Stakeholders.API.Domain.Models;

public class Blog
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty; 
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> ImageUrls { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty; 
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}