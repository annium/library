using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Abstractions.Connectors.User;

/// <summary>
/// Hands out leases on user connectors shared by settings, building a connector on the first lease and
/// tearing it down once the last one is returned.
/// </summary>
/// <remarks>
/// A provider meters what a connector costs it - request weight, order rate, open sockets and listen keys -
/// per account, not per connector, so callers that would each open their own connector for the same account
/// are better off sharing one. Depend on this factory rather than on <see cref="IUserConnectorFactory"/> to
/// say that the connector is shared: the alternative, a second method on the plain factory, hides a
/// different ownership model behind a call site.
/// </remarks>
public interface IPooledUserConnectorFactory
{
    /// <summary>
    /// Takes a lease on the connector shared by everything using these settings, building it on the first
    /// lease.
    /// </summary>
    /// <param name="settings">The user settings identifying the provider account to connect to.</param>
    /// <returns>A lease on the shared user connector; disposing it gives the lease back.</returns>
    IUserConnector Create(UserSettings settings);
}
