using System.Security.Cryptography;
using Tours.API.Domain.Enums;

namespace Tours.API.Domain.Models;

public class Tour
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal Price { get; set; } 
    public int DurationHours { get; set; }

    public int Difficulty { get; set; }
    public List<string> Tags { get; set; } = new();
    public TourStatus Status { get; set; } 

    public List<Keypoint> Keypoints { get; set; } = new();
}