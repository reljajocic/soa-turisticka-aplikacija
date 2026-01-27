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
        // Provera da li tura uopšte postoji pre startovanja
        var tour = await _ctx.Tours.Find(x => x.Id == dto.TourId).FirstOrDefaultAsync();
        if (tour == null) throw new Exception("Tour not found");

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
        try
        {
            // 1. Nađi sesiju
            var e = await _ctx.Executions.Find(x => x.Id == dto.ExecutionId && x.UserId == userId && x.Status == "Active").FirstOrDefaultAsync();
            if (e is null) return new { error = "No active execution found for this user." };

            // 2. Nađi turu
            var tour = await _ctx.Tours.Find(x => x.Id == e.TourId).FirstOrDefaultAsync();

            // ZAŠTITA OD NULL-a
            if (tour is null) return new { error = "Tour not found in database." };
            if (tour.Keypoints == null) return new { error = "Tour has NULL keypoints list." };
            if (tour.Keypoints.Count == 0) return new { error = "Tour has EMPTY keypoints list." };

            // 3. Nađi poziciju
            var p = await _ctx.Positions.Find(x => x.UserId == userId).FirstOrDefaultAsync();
            if (p is null) return new { error = "GPS Position not found. Please use the Simulator to set location!" };

            // 4. Provera indexa
            if (e.ProgressIndex >= tour.Keypoints.Count)
                return new { reached = false, progress = tour.Keypoints.Count, distanceKm = 0 };

            var target = tour.Keypoints[e.ProgressIndex];

            // 5. Matematika
            var dist = Geo.Haversine(p.Lat, p.Lon, target.Lat, target.Lon);

            // 6. Provera blizine (manje od 50m)
            if (dist < 0.05)
            {
                var newIdx = e.ProgressIndex + 1;
                var upd = Builders<TourExecution>.Update
                  .Set(x => x.ProgressIndex, newIdx)
                  .Set(x => x.LastActivity, DateTime.UtcNow);

                await _ctx.Executions.UpdateOneAsync(x => x.Id == e.Id, upd);
                return new { reached = true, progress = newIdx, distanceKm = 0 };
            }

            return new { reached = false, progress = e.ProgressIndex, distanceKm = dist };
        }
        catch (Exception ex)
        {
            // OVO JE KLJUČNO: Vraćamo grešku frontend-u da vidimo šta je!
            Console.WriteLine($"CRASH U POLLASYNC: {ex.Message} \n {ex.StackTrace}");
            return new { error = $"SERVER ERROR: {ex.Message}" };
        }
    }

    public async Task FinishAsync(Guid userId, FinishDto dto)
    {
        var upd = Builders<TourExecution>.Update
          .Set(x => x.Status, dto.Success ? "Completed" : "Abandoned")
          .Set(x => x.FinishedAt, DateTime.UtcNow);

        await _ctx.Executions.UpdateOneAsync(x => x.Id == dto.ExecutionId && x.UserId == userId, upd);
    }
}