using System.Security.Cryptography;

namespace Tours.API.Domain.Models;

public class Tour
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationHours { get; set; }
    public List<Keypoint> Keypoints { get; set; } = new();
}