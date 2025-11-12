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
        var e = new TourExecution
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TourId = dto.TourId,
            StartedAt = DateTime.UtcNow,
            Status = "Active",
            ProgressIndex = 0,
            LastActivity = DateTime.UtcNow
        };
        await _ctx.Executions.InsertOneAsync(e);
        return e.Id;
    }

    public async Task<object> PollAsync(Guid userId, PollDto dto)
    {
        var e = await _ctx.Executions.Find(x => x.Id == dto.ExecutionId && x.UserId == userId && x.Status == "Active").FirstOrDefaultAsync();
        if (e is null) return new { error = "No active exec" };

        var tour = await _ctx.Tours.Find(x => x.Id == e.TourId).FirstOrDefaultAsync();
        if (tour is null || tour.Keypoints.Count == 0) return new { error = "No keypoints" };

        var p = await _ctx.Positions.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        if (p is null) return new { error = "No position" };

        var idx = Math.Min(e.ProgressIndex, tour.Keypoints.Count - 1);
        var target = tour.Keypoints[idx];
        var dist = Geo.Haversine(p.Lat, p.Lon, target.Lat, target.Lon);

        if (dist < 0.03)
        {
            var newIdx = Math.Min(e.ProgressIndex + 1, tour.Keypoints.Count);
            var upd = Builders<TourExecution>.Update
              .Set(x => x.ProgressIndex, newIdx).Set(x => x.LastActivity, DateTime.UtcNow);
            await _ctx.Executions.UpdateOneAsync(x => x.Id == e.Id, upd);
            return new { reached = true, progress = newIdx };
        }
        return new { reached = false, progress = e.ProgressIndex, distanceKm = dist };
    }

    public async Task FinishAsync(Guid userId, FinishDto dto)
    {
        var upd = Builders<TourExecution>.Update
          .Set(x => x.Status, dto.Success ? "Completed" : "Abandoned")
          .Set(x => x.FinishedAt, DateTime.UtcNow);
        await _ctx.Executions.UpdateOneAsync(x => x.Id == dto.ExecutionId && x.UserId == userId, upd);
    }
}