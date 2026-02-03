using MongoDB.Driver;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;
using Tours.API.Infrastructure;

namespace Tours.API.Application.Services;

public class ExecutionService : IExecutionService
{
    private readonly TourMongoContext _ctx;
    public ExecutionService(TourMongoContext ctx) => _ctx = ctx;

    public async Task<Guid> StartAsync(Guid userId, StartExecDto dto)
    {
        // 1. Provera da li tura postoji
        var tour = await _ctx.Tours.Find(x => x.Id == dto.TourId).FirstOrDefaultAsync();
        if (tour == null) throw new Exception("Tour not found");

        // 2. PROVERA KUPOVINE (Specifikacija tačka 17: "preduslov za pokretanje je da je kupljena")
        var token = await _ctx.Tokens.Find(x => x.TourId == dto.TourId && x.UserId == userId).FirstOrDefaultAsync();
        if (token == null) throw new Exception("You must purchase the tour before starting it.");

        // 3. Provera da li već postoji aktivna sesija (opciono, ali dobro za stabilnost)
        var existing = await _ctx.Executions.Find(x => x.UserId == userId && x.TourId == dto.TourId && x.Status == "Active").FirstOrDefaultAsync();
        if (existing != null) return existing.Id;

        var e = new TourExecution
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TourId = dto.TourId,
            StartedAt = DateTime.UtcNow,
            Status = "Active",
            ProgressIndex = 0,
            LastActivity = DateTime.UtcNow,
            KeypointArrivalTimes = new List<DateTime>()
        };

        await _ctx.Executions.InsertOneAsync(e);
        return e.Id;
    }

    public async Task<object> PollAsync(Guid userId, PollDto dto)
    {
        try
        {
            var e = await _ctx.Executions.Find(x => x.Id == dto.ExecutionId && x.UserId == userId && x.Status == "Active").FirstOrDefaultAsync();
            if (e is null) return new { error = "No active execution found." };

            var tour = await _ctx.Tours.Find(x => x.Id == e.TourId).FirstOrDefaultAsync();
            if (tour is null || tour.Keypoints == null || tour.Keypoints.Count == 0)
                return new { error = "Invalid tour data." };

            var p = await _ctx.Positions.Find(x => x.UserId == userId).FirstOrDefaultAsync();
            if (p is null) return new { error = "GPS Position not found. Use Simulator!" };

            var baseUpdate = Builders<TourExecution>.Update.Set(x => x.LastActivity, DateTime.UtcNow);

            if (e.ProgressIndex >= tour.Keypoints.Count)
            {
                await _ctx.Executions.UpdateOneAsync(x => x.Id == e.Id, baseUpdate);
                return new { reached = false, progress = tour.Keypoints.Count, distanceKm = 0, message = "Tour already completed.", arrivalTimes = e.KeypointArrivalTimes };
            }

            var target = tour.Keypoints[e.ProgressIndex];
            var dist = Geo.Haversine(p.Lat, p.Lon, target.Lat, target.Lon);

            if (dist < 0.05)
            {
                var arrivalTime = DateTime.UtcNow;
                var newIdx = e.ProgressIndex + 1;

                var update = baseUpdate
                    .Set(x => x.ProgressIndex, newIdx)
                    .Push(x => x.KeypointArrivalTimes, arrivalTime);

                if (newIdx == tour.Keypoints.Count)
                {
                    update = update.Set(x => x.Status, "Completed").Set(x => x.FinishedAt, arrivalTime);
                }

                await _ctx.Executions.UpdateOneAsync(x => x.Id == e.Id, update);
                return new { reached = true, progress = newIdx, distanceKm = 0, completed = newIdx == tour.Keypoints.Count, arrivalTimes = e.KeypointArrivalTimes };
            }

            await _ctx.Executions.UpdateOneAsync(x => x.Id == e.Id, baseUpdate);
            return new { reached = false, progress = e.ProgressIndex, distanceKm = dist, arrivalTimes = e.KeypointArrivalTimes };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"POLL ERROR: {ex.Message}");
            return new { error = $"SERVER ERROR: {ex.Message}" };
        }
    }

    public async Task FinishAsync(Guid userId, FinishDto dto)
    {
        // Specifikacija: "Sesija se završava kada turista završi (completed) ili napusti (abandoned)"
        // Ako frontend šalje Success=true, status je Completed, inače Abandoned
        var status = dto.Success ? "Completed" : "Abandoned";

        var upd = Builders<TourExecution>.Update
          .Set(x => x.Status, status)
          .Set(x => x.FinishedAt, DateTime.UtcNow)
          .Set(x => x.LastActivity, DateTime.UtcNow);

        await _ctx.Executions.UpdateOneAsync(x => x.Id == dto.ExecutionId && x.UserId == userId, upd);
    }

    public async Task<List<TourExecution>> GetUserExecutionsAsync(Guid userId)
    {
        return await _ctx.Executions.Find(x => x.UserId == userId).ToListAsync();
    }
}