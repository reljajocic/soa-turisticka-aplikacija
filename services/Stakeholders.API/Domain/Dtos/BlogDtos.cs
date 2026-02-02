using System.ComponentModel.DataAnnotations;

namespace Stakeholders.API.DTOs;

public class CreateBlogDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public List<string>? ImageUrls { get; set; }
}

public class CreateCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
}

public class BlogDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Username { get; set; }
    public string AuthorAvatarUrl { get; set; } 
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; }
    public List<CommentDto> Comments { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string AuthorAvatarUrl { get; set; } // <--- NOVO
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}