namespace Tours.API.Domain.Models;

public class UserPosition
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public DateTime UpdatedAt { get; set; }
}
