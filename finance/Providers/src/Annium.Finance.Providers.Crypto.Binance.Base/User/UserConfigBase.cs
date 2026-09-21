using System;
using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Crypto.Binance.Base.User;

/// <summary>Base configuration for a Binance account/trading connector: credentials and the user HTTP/WebSocket API endpoints.</summary>
/// <remarks>
/// It carries no user-stream mechanism, and deliberately so: the venues do not share one. One reaches its
/// account stream through a key fetched over REST and spent in a URL, the other through a signed subscription
/// on a request/response socket, and a base holding either of them makes the other venue configure machinery
/// it never resolves. That is what it did until 2026-09-21, on the venue that had not implemented a stream
/// at all - which is exactly the case where nothing says so.
/// </remarks>
public abstract record UserConfigBase
{
    /// <summary>Gets the name of the provider to connect to.</summary>
    public required string Provider { get; init; }

    /// <summary>Gets the API key identifying the account to Binance.</summary>
    public required string Key { get; init; }

    /// <summary>Gets the API secret used to sign authenticated requests to Binance.</summary>
    public required string Secret { get; init; }

    /// <summary>Gets the base URI of the account/trading HTTP API.</summary>
    public required Uri HttpApi { get; init; }

    /// <summary>Gets the base URI of the user data stream WebSocket API.</summary>
    public required Uri WsApi { get; init; }
}

/// <summary>Extension methods for converting a <see cref="UserConfigBase"/> into a <see cref="UserSettings"/>.</summary>
public static class UserConfigBaseExtensions
{
    /// <summary>Extracts the provider, environment and credentials from a user configuration into a <see cref="UserSettings"/>.</summary>
    /// <param name="config">The user configuration to extract settings from.</param>
    /// <returns>The extracted user settings.</returns>
    public static UserSettings GetSettings(this UserConfigBase config) =>
        new()
        {
            Provider = config.Provider,
            Key = config.Key,
            Secret = config.Secret,
        };
}
