using Tours.API.Domain.Enums;
using Tours.API.Domain.Models;

namespace Tours.API.Domain.Dtos;

public record CreateTourDto(string Name, string? Description, int Difficulty, List<string> Tags, int Status, string? ImageUrl);
public record ChangeStatusDto(int Status, double? Price);
public record CreateKeypointDto(string Name, double Latitude, double Longitude, string? Description, string? Image);
public record SetPosDto(double Latitude, double Longitude);
public record AddCartDto(Guid TourId, string Name, double Price);

public class TourDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Difficulty { get; set; }
    public List<string> Tags { get; set; } = new();
    public TourStatus Status { get; set; }
    public double Price { get; set; }
    public Guid AuthorId { get; set; }
    public List<Keypoint> Keypoints { get; set; } = new();
}