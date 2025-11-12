namespace Followers.API.Domain.Models;

public record FollowCommand(Guid FollowerId, Guid FollowedId);
public record FollowCheck(Guid FollowerId, Guid AuthorId);

public record FollowResult(bool Success);
public record IsFollowingResult(bool IsFollowing);
