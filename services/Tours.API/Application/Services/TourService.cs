using MongoDB.Driver;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;
using Tours.API.Infrastructure;

namespace Tours.API.Application.Services;

public class TourService : ITourService
{
    private readonly TourMongoContext _ctx;
    public TourService(TourMongoContext ctx) => _ctx = ctx;

    public async Task<Guid> CreateAsync(Guid authorId, CreateTourDto dto)
    {
        var t = new Tour
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DurationHours = dto.DurationHours
        };
        await _ctx.Tours.InsertOneAsync(t);
        return t.Id;
    }

    public async Task<bool> AddKeypointAsync(Guid tourId, CreateKeypointDto dto)
    {
        var upd = Builders<Tour>.Update.Push(x => x.Keypoints,
          new Keypoint { Name = dto.Name, Lat = dto.Lat, Lon = dto.Lon, Description = dto.Description, ImageUrl = dto.ImageUrl });
        var res = await _ctx.Tours.UpdateOneAsync(t => t.Id == tourId, upd);
        return res.MatchedCount > 0;
    }
}