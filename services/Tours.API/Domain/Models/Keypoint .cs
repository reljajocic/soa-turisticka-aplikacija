namespace Tours.API.Domain.Models;

public class Keypoint
{
    public string Name { get; set; } = default!;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}