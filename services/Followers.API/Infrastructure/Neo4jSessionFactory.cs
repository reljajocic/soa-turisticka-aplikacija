using Neo4j.Driver;

namespace Followers.API.Infrastructure;

public sealed class Neo4jSessionFactory : IAsyncDisposable
{
    private readonly IDriver _driver;
    public Neo4jSessionFactory(string uri, string user, string pass)
        => _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass));

    public IAsyncSession OpenSession()
        => _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));

    public async ValueTask DisposeAsync() => await _driver.DisposeAsync();
}
