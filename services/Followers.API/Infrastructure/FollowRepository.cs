using Followers.API.Domain.Models;
using Neo4j.Driver;

namespace Followers.API.Infrastructure;

public class FollowRepository
{
    private readonly Neo4jSessionFactory _factory;
    public FollowRepository(Neo4jSessionFactory factory) => _factory = factory;

    public async Task CreateFollowAsync(FollowCommand cmd, CancellationToken ct)
    {
        await using var session = _factory.OpenSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            var query = @"
                MERGE (a:User {id: $follower})
                MERGE (b:User {id: $followed})
                MERGE (a)-[:FOLLOWS]->(b)";
            await tx.RunAsync(query, new { follower = cmd.FollowerId.ToString(), followed = cmd.FollowedId.ToString() });
        });
    }

    public async Task DeleteFollowAsync(FollowCommand cmd, CancellationToken ct)
    {
        await using var session = _factory.OpenSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            var query = @"
                MATCH (a:User {id:$follower})-[r:FOLLOWS]->(b:User {id:$followed})
                DELETE r";
            await tx.RunAsync(query, new { follower = cmd.FollowerId.ToString(), followed = cmd.FollowedId.ToString() });
        });
    }

    public async Task<bool> IsFollowingAsync(FollowCheck check, CancellationToken ct)
    {
        await using var session = _factory.OpenSession();
        var cursor = await session.RunAsync(@"
            MATCH (a:User {id:$f})-[:FOLLOWS]->(b:User {id:$a})
            RETURN count(*) AS c",
            new { f = check.FollowerId.ToString(), a = check.AuthorId.ToString() });

        var rec = await cursor.SingleAsync();
        var count = rec["c"].As<long>();
        return count > 0;
    }
}
