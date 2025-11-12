using Followers.API.Domain.Models;

namespace Followers.API.Application.Interfaces;

public interface IFollowService
{
    Task<FollowResult> FollowAsync(FollowCommand cmd, CancellationToken ct);
    Task<FollowResult> UnfollowAsync(FollowCommand cmd, CancellationToken ct);
    Task<IsFollowingResult> IsFollowingAsync(FollowCheck check, CancellationToken ct);
}
