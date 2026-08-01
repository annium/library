using System.Threading;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration.Messages.Requests;

namespace Annium.Integrations.Social.Telegram.Integration.Messages;

/// <summary>
/// Message-related operations of the Telegram Bot API, such as sending text messages to a chat.
/// </summary>
public interface IMessageApi
{
    /// <summary>
    /// Sends a text message to a chat via the Telegram <c>sendMessage</c> endpoint.
    /// </summary>
    /// <param name="request">The chat and text to send.</param>
    /// <param name="ct">The token used to cancel the request.</param>
    /// <returns><see langword="true"/> if Telegram accepted the message; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> SendMessageAsync(SendMessageRequest request, CancellationToken ct = default);
}
