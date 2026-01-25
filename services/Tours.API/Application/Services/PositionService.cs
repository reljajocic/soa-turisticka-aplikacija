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
        var filter = Builders<UserPosition>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<UserPosition>.Update
          .Set(x => x.Lat, dto.Latitude)
          .Set(x => x.Lon, dto.Longitude)
          .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _ctx.Positions.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<UserPosition?> GetAsync(Guid userId)
      => await _ctx.Positions.Find(x => x.UserId == userId).FirstOrDefaultAsync();
}