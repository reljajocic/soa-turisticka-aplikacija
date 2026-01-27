using MongoDB.Driver;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;
using Tours.API.Infrastructure;

namespace Tours.API.Application.Services;

public class PositionService : IPositionService
{
    private readonly TourMongoContext _ctx;
    public PositionService(TourMongoContext ctx) => _ctx = ctx;

    public async Task SetAsync(Guid userId, SetPosDto dto)
    {
        // 1. Prvo probamo da nađemo postojeću poziciju
        var existing = await _ctx.Positions.Find(x => x.UserId == userId).FirstOrDefaultAsync();

        if (existing == null)
        {
            // 2. Ako ne postoji, pravimo novu sa NAŠIM Guid-om (ne ObjectId)
            var newPos = new UserPosition
            {
                Id = Guid.NewGuid(), // <--- MI GENERIŠEMO ID
                UserId = userId,
                Lat = dto.Latitude,
                Lon = dto.Longitude,
                UpdatedAt = DateTime.UtcNow
            };
            await _ctx.Positions.InsertOneAsync(newPos);
        }
        else
        {
            // 3. Ako postoji, samo ažuriramo koordinate
            var update = Builders<UserPosition>.Update
              .Set(x => x.Lat, dto.Latitude)
              .Set(x => x.Lon, dto.Longitude)
              .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _ctx.Positions.UpdateOneAsync(x => x.Id == existing.Id, update);
        }
    }

    public async Task<UserPosition?> GetAsync(Guid userId)
      => await _ctx.Positions.Find(x => x.UserId == userId).FirstOrDefaultAsync();
}