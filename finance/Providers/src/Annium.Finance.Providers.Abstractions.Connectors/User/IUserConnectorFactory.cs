using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Abstractions.Connectors.User;

/// <summary>
/// Creates <see cref="IUserConnector"/> instances, resolving all their dependencies through the container
/// (used to build standalone connectors, e.g. registered as singletons in DI).
/// </summary>
public interface IUserConnectorFactory
{
    /// <summary>
    /// Creates a user connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The user settings identifying the provider account to connect to.</param>
    /// <returns>A new user connector instance the caller owns; disposing it tears it down.</returns>
    IUserConnector Create(UserSettings settings);

    /// <summary>
    /// Takes a lease on the connector shared by everything using these settings, building it on the
    /// first lease.
    /// </summary>
    /// <remarks>
    /// A provider charges its rate limit per account rather than per connector, so callers that each
    /// want a connector for the same settings should share one instead of opening their own. Disposing
    /// the returned value gives the lease back; the connector itself is torn down once the last lease
    /// is returned.
    /// </remarks>
    /// <param name="settings">The user settings identifying the provider account to connect to.</param>
    /// <returns>A lease on the shared user connector.</returns>
    IUserConnector CreatePooled(UserSettings settings);
}
