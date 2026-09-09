using Annium.Finance.Providers.Core.Shared;
using Annium.Logging;

namespace Annium.Finance.Providers.Core.Internal.Shared.Channels;

internal static class ConnectorChannel
{
    public static IConnectorChannel<T> Create<T>(ConnectorDelivery delivery, ILogger logger) =>
        delivery is ConnectorDelivery.Inline ? new InlineChannel<T>() : new BufferedChannel<T>(logger);
}
