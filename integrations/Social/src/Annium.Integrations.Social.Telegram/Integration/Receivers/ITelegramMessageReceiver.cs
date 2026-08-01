using System.Threading.Channels;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

namespace Annium.Integrations.Social.Telegram.Integration.Receivers;

/// <summary>
/// Supplies a stream of incoming Telegram updates, sourced from either long-polling or a webhook.
/// </summary>
public interface ITelegramMessageReceiver
{
    /// <summary>
    /// Gets the channel reader that yields updates as they are received; completes when the receiver stops.
    /// </summary>
    ChannelReader<Update> Updates { get; }
}
