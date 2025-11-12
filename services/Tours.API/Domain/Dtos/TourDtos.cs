namespace Tours.API.Domain.Dtos;

public record CreateTourDto(string Name, string? Description, decimal Price, int DurationHours);
public record CreateKeypointDto(string Name, double Lat, double Lon, string? Description, string? ImageUrl);
public record SetPosDto(double Lat, double Lon);
public record AddCartDto(Guid TourId, string Name, decimal Price);