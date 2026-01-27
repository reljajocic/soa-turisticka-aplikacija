using MongoDB.Driver;
using Tours.API.Application.Interfaces;
using Tours.API.Domain.Dtos;
using Tours.API.Domain.Enums;
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
            Difficulty = dto.Difficulty,
            Tags = dto.Tags ?? new List<string>(),
            Price = (decimal)dto.Price,
            Status = (TourStatus)dto.Status,
            Keypoints = new List<Keypoint>()
        };

        await _ctx.Tours.InsertOneAsync(t);
        return t.Id;
    }

    public async Task<bool> AddKeypointAsync(Guid tourId, CreateKeypointDto dto)
    {
        var upd = Builders<Tour>.Update.Push(x => x.Keypoints,
          new Keypoint
          {
              Name = dto.Name,
              Lat = dto.Latitude,
              Lon = dto.Longitude,
              Description = dto.Description,
              ImageUrl = dto.Image
          });

        var res = await _ctx.Tours.UpdateOneAsync(t => t.Id == tourId, upd);
        return res.MatchedCount > 0;
    }

    public async Task<IEnumerable<TourDto>> GetAllAsync()
    {
        // Vraća samo OBJAVLJENE ture (za turiste)
        var tours = await _ctx.Tours.Find(t => t.Status == TourStatus.Published).ToListAsync();
        return MapToDto(tours);
    }

    public async Task<IEnumerable<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        var tours = await _ctx.Tours.Find(t => t.AuthorId == authorId).ToListAsync();
        return MapToDto(tours);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, TourStatus status)
    {
        var upd = Builders<Tour>.Update.Set(t => t.Status, status);
        var res = await _ctx.Tours.UpdateOneAsync(t => t.Id == id, upd);
        return res.MatchedCount > 0;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid authorId, CreateTourDto dto)
    {
        var update = Builders<Tour>.Update
            .Set(t => t.Name, dto.Name)
            .Set(t => t.Description, dto.Description)
            .Set(t => t.Difficulty, dto.Difficulty)
            .Set(t => t.Tags, dto.Tags ?? new List<string>())
            .Set(t => t.Price, (decimal)dto.Price)
            .Set(t => t.Status, (TourStatus)dto.Status);

        var res = await _ctx.Tours.UpdateOneAsync(t => t.Id == id && t.AuthorId == authorId, update);
        return res.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid authorId)
    {
        var res = await _ctx.Tours.DeleteOneAsync(t => t.Id == id && t.AuthorId == authorId);
        return res.DeletedCount > 0;
    }

    private IEnumerable<TourDto> MapToDto(List<Tour> tours)
    {
        return tours.Select(t => new TourDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description ?? "",
            Difficulty = t.Difficulty,
            Tags = t.Tags,
            Status = t.Status,
            Price = (double)t.Price,
            AuthorId = t.AuthorId,
            Keypoints = t.Keypoints 
        });
    }

    public async Task<TourDto?> GetAsync(Guid id)
    {
        var t = await _ctx.Tours.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (t == null) return null;

        return new TourDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description ?? "",
            Difficulty = t.Difficulty,
            Tags = t.Tags,
            Status = t.Status,
            Price = (double)t.Price,
            AuthorId = t.AuthorId,
            Keypoints = t.Keypoints // Vraćamo tačke!
        };
    }

    public async Task<IEnumerable<TourDto>> GetPurchasedToursAsync(Guid userId)
    {
        // 1. Nađi sve tokene za ovog korisnika
        var tokens = await _ctx.Tokens.Find(t => t.UserId == userId).ToListAsync();

        if (tokens.Count == 0) return new List<TourDto>();

        // 2. Izdvoj ID-jeve tura
        var tourIds = tokens.Select(t => t.TourId).ToList();

        // 3. Nađi te ture u bazi
        var tours = await _ctx.Tours.Find(t => tourIds.Contains(t.Id)).ToListAsync();

        return MapToDto(tours);
    }
}