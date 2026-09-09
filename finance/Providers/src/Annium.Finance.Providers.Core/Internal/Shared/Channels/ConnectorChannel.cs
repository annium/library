using Annium.Finance.Providers.Core.Shared;
using Annium.Logging;

namespace Annium.Finance.Providers.Core.Internal.Shared.Channels;

/// <summary>
/// Builds the channel a connector fans its values out through.
/// </summary>
/// <remarks>
/// The single place that turns a <see cref="ConnectorDelivery"/> into an implementation, so neither
/// connector base repeats the choice - the user base was making it once per stream.
/// </remarks>
internal static class ConnectorChannel
{
    /// <summary>
    /// Creates the channel for the given delivery mode.
    /// </summary>
    /// <typeparam name="T">The type of value carried.</typeparam>
    /// <param name="delivery">How written values are to reach the subscribers.</param>
    /// <param name="logger">The logger the buffered implementation pumps values under.</param>
    /// <returns>An inline channel for <see cref="ConnectorDelivery.Inline"/>, a buffered one otherwise.</returns>
    public static IConnectorChannel<T> Create<T>(ConnectorDelivery delivery, ILogger logger) =>
        delivery is ConnectorDelivery.Inline ? new InlineChannel<T>() : new BufferedChannel<T>(logger);
}
