using Stakeholders.API.Application.Interfaces;
using Stakeholders.API.Domain.Models;
using Stakeholders.API.Infrastructure;
using MongoDB.Driver;

namespace Stakeholders.API.Application.Services;

public class ProfileService : IProfileService
{
    private readonly MongoContext _ctx;
    public ProfileService(MongoContext ctx) => _ctx = ctx;

    public async Task<User?> GetMeAsync(Guid userId)
        => await _ctx.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

    public async Task<bool> UpdateProfileAsync(Guid userId, string? first, string? last, string? bio, string? motto, string? avatar)
    {
        var update = Builders<User>.Update
            .Set(u => u.FirstName, first)
            .Set(u => u.LastName, last)
            .Set(u => u.Bio, bio)
            .Set(u => u.Motto, motto)
            .Set(u => u.AvatarUrl, avatar);
        var res = await _ctx.Users.UpdateOneAsync(u => u.Id == userId, update);
        return res.ModifiedCount > 0;
    }
}
