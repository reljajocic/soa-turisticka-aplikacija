using Tours.API.Domain.Enums;

namespace Tours.API.Domain.Dtos;

public record CreateTourDto(string Name, string? Description, int Difficulty, List<string> Tags, double Price, int Status);
public record CreateKeypointDto(string Name, double Latitude, double Longitude, string? Description, string? Image);
public record SetPosDto(double Latitude, double Longitude);
public record AddCartDto(Guid TourId, string Name, double Price);

public class TourDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Difficulty { get; set; }
    public List<string> Tags { get; set; }
    public TourStatus Status { get; set; } 
    public double Price { get; set; }
    public Guid AuthorId { get; set; }
}