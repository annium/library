using Annium.Finance.Providers.Abstractions.Domain.User;

namespace Annium.Finance.Providers.Abstractions.Connectors.User;

/// <summary>
/// Creates standalone <see cref="IUserConnector"/> instances, each owning everything it is built from.
/// </summary>
/// <remarks>
/// Resolve this from the scope the connector belongs to: the factory builds in whatever provider it was
/// resolved through, so a caller that runs work in its own scope gets connectors wired to that scope's
/// services, and a caller with no scope of its own gets the container's.
/// </remarks>
public interface IUserConnectorFactory
{
    /// <summary>
    /// Creates a user connector configured with the given settings.
    /// </summary>
    /// <param name="settings">The user settings identifying the provider account to connect to.</param>
    /// <returns>A new user connector instance the caller owns; disposing it tears it down.</returns>
    IUserConnector Create(UserSettings settings);
}
