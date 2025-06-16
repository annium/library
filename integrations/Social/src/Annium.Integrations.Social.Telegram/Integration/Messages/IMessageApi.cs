using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration.Messages.Requests;

namespace Annium.Integrations.Social.Telegram.Integration.Messages;

public interface IMessageApi
{
    ValueTask<bool> SendMessageAsync(SendMessageRequest request);
}
