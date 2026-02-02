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
            ImageUrl = dto.ImageUrl,
            Difficulty = dto.Difficulty,
            Tags = dto.Tags ?? new List<string>(),
            Price = 0,
            Status = TourStatus.Draft, 

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
              ImageUrl = dto.Image // Ovo je već postojalo, super
          });

        var res = await _ctx.Tours.UpdateOneAsync(t => t.Id == tourId, upd);
        return res.MatchedCount > 0;
    }

    public async Task<IEnumerable<TourDto>> GetAllAsync()
    {
        var tours = await _ctx.Tours.Find(t => t.Status == TourStatus.Published).ToListAsync();
        return MapToDto(tours);
    }

    public async Task<IEnumerable<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        var tours = await _ctx.Tours.Find(t => t.AuthorId == authorId).ToListAsync();
        return MapToDto(tours);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, TourStatus status, double? price)
    {
        var updateDef = Builders<Tour>.Update.Set(t => t.Status, status);

        // Ako je prosleđena cena (npr. kod Publish ili Archive), ažuriraj je
        if (price.HasValue)
        {
            updateDef = updateDef.Set(t => t.Price, (decimal)price.Value);
        }

        var res = await _ctx.Tours.UpdateOneAsync(t => t.Id == id, updateDef);
        return res.MatchedCount > 0;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid authorId, CreateTourDto dto)
    {
        var update = Builders<Tour>.Update
            .Set(t => t.Name, dto.Name)
            .Set(t => t.Description, dto.Description)
            .Set(t => t.ImageUrl, dto.ImageUrl)
            .Set(t => t.Difficulty, dto.Difficulty)
            .Set(t => t.Tags, dto.Tags ?? new List<string>());

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
            ImageUrl = t.ImageUrl,
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
            ImageUrl = t.ImageUrl,
            Difficulty = t.Difficulty,
            Tags = t.Tags,
            Status = t.Status,
            Price = (double)t.Price,
            AuthorId = t.AuthorId,
            Keypoints = t.Keypoints
        };
    }

    public async Task<IEnumerable<TourDto>> GetPurchasedToursAsync(Guid userId)
    {
        var tokens = await _ctx.Tokens.Find(t => t.UserId == userId).ToListAsync();
        if (tokens.Count == 0) return new List<TourDto>();
        var tourIds = tokens.Select(t => t.TourId).ToList();
        var tours = await _ctx.Tours.Find(t => tourIds.Contains(t.Id)).ToListAsync();
        return MapToDto(tours);
    }
}