namespace Annium.linq2db.PostgreSql;

/// <summary>
/// Configuration settings for PostgreSQL database connections
/// </summary>
public record PostgreSqlConfiguration
{
    /// <summary>
    /// Gets or sets the PostgreSQL server hostname or IP address
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PostgreSQL server port number
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the name of the database to connect to
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for database authentication
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for database authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of connections the pool holds open regardless of demand.
    /// </summary>
    /// <remarks>
    /// Zero, so a service opens what it uses. This was hard-coded at 100, per data source - and a host
    /// registers one per database it talks to, so three of them meant three hundred connections the pool
    /// was obliged to hold. Together with a lifetime that retired each after three minutes, that is a
    /// hundred reconnections a minute made to satisfy the setting rather than any query, which is enough
    /// to time out against a local database that is otherwise idle.
    /// </remarks>
    public int MinPoolSize { get; set; }

    /// <summary>
    /// Gets or sets the ceiling on connections the pool will open.
    /// </summary>
    public int MaxPoolSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets how long, in seconds, a pooled connection may live before it is retired and replaced.
    /// </summary>
    /// <remarks>
    /// Npgsql's own default. The 180 seconds this used to force is only worth paying where something in
    /// front of the database moves connections around; on its own it buys nothing and costs a reconnection
    /// per connection every three minutes.
    /// </remarks>
    public int ConnectionLifetime { get; set; } = 3600;

    /// <summary>
    /// Gets the PostgreSQL connection string built from the configuration properties
    /// </summary>
    public virtual string ConnectionString =>
        string.Join(
            ';',
            $"Host={Host}",
            $"Port={Port}",
            $"Database={Database}",
            $"Username={User}",
            $"Password={Password}",
            "SSL Mode=Prefer",
            "Trust Server Certificate=true",
            "Keepalive=30",
            "Pooling=true",
            $"Connection Lifetime={ConnectionLifetime}",
            $"Minimum Pool Size={MinPoolSize}",
            $"Maximum Pool Size={MaxPoolSize}"
        );
}
