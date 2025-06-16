using System.Threading.Channels;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

namespace Annium.Integrations.Social.Telegram.Integration.Receivers;

public interface ITelegramMessageReceiver
{
    ChannelReader<Update> Updates { get; }
}
