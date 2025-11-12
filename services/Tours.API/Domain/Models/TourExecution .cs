namespace Tours.API.Domain.Models;

public class TourExecution
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TourId { get; set; }
    public DateTime StartedAt { get; set; }
    public string Status { get; set; } = "Active";
    public int ProgressIndex { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime? FinishedAt { get; set; }
}