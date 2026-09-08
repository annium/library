using Annium.Testing;
using Xunit;

namespace Annium.linq2db.PostgreSql.Tests;

/// <summary>
/// Pins the pooling the connection string asks for. These are defaults every service that talks to Postgres
/// inherits, and the pool acts on them whether or not any query needs it to.
/// </summary>
public class PostgreSqlConfigurationTests
{
    /// <summary>
    /// By default the pool holds nothing open that nothing is using.
    /// </summary>
    /// <remarks>
    /// A minimum of 100 was hard-coded here, per data source, and a host registers one per database it talks
    /// to - three of them meant three hundred connections the pool was obliged to hold and, with the lifetime
    /// below, to replace every three minutes. That is a hundred reconnections a minute made to satisfy the
    /// setting rather than any query, and it was enough to time them out against an idle local database.
    /// </remarks>
    [Fact]
    public void Default_HoldsNoIdleConnections()
    {
        // arrange
        var cfg = new PostgreSqlConfiguration();

        // assert
        cfg.MinPoolSize.Is(0);
        cfg.ConnectionString.Contains("Minimum Pool Size=0").IsTrue(cfg.ConnectionString);
    }

    /// <summary>
    /// By default a connection is not retired on a timer. Recycling every few minutes is worth paying for
    /// where something in front of the database moves connections around, and costs a reconnection per
    /// connection otherwise.
    /// </summary>
    [Fact]
    public void Default_DoesNotRecycleConnectionsOnAShortTimer()
    {
        // arrange
        var cfg = new PostgreSqlConfiguration();

        // assert
        cfg.ConnectionLifetime.Is(3600);
        cfg.ConnectionString.Contains("Connection Lifetime=3600").IsTrue(cfg.ConnectionString);
    }

    /// <summary>
    /// A service that does want a warm pool, or a shorter recycle, can still ask for one.
    /// </summary>
    [Fact]
    public void Pooling_IsConfigurable()
    {
        // arrange
        var cfg = new PostgreSqlConfiguration
        {
            MinPoolSize = 5,
            MaxPoolSize = 50,
            ConnectionLifetime = 180,
        };

        // assert
        var connectionString = cfg.ConnectionString;
        connectionString.Contains("Minimum Pool Size=5").IsTrue(connectionString);
        connectionString.Contains("Maximum Pool Size=50").IsTrue(connectionString);
        connectionString.Contains("Connection Lifetime=180").IsTrue(connectionString);
    }
}
