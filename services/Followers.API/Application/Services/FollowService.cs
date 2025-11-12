using Followers.API.Application.Interfaces;
using Followers.API.Domain.Models;
using Followers.API.Infrastructure;

namespace Followers.API.Application.Services;

public class FollowService : IFollowService
{
    private readonly FollowRepository _repo;
    public FollowService(FollowRepository repo) => _repo = repo;

    public async Task<FollowResult> FollowAsync(FollowCommand cmd, CancellationToken ct)
    {
        if (cmd.FollowerId == cmd.FollowedId) return new(false);
        await _repo.CreateFollowAsync(cmd, ct);
        return new(true);
    }

    public async Task<FollowResult> UnfollowAsync(FollowCommand cmd, CancellationToken ct)
    {
        if (cmd.FollowerId == cmd.FollowedId) return new(false);
        await _repo.DeleteFollowAsync(cmd, ct);
        return new(true);
    }

    public async Task<IsFollowingResult> IsFollowingAsync(FollowCheck check, CancellationToken ct)
    {
        var ok = await _repo.IsFollowingAsync(check, ct);
        return new(ok);
    }
}
