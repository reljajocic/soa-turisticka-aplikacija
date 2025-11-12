namespace Stakeholders.API.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = "Tourist";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? Motto { get; set; }
    public string? AvatarUrl { get; set; }
}
