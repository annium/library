using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Messages;

namespace Annium.Integrations.Social.Telegram.Internal.Integration;

/// <summary>
/// Default <see cref="ITelegramApi"/> implementation that exposes the configured <see cref="IMessageApi"/> for
/// sending messages.
/// </summary>
/// <param name="Messages">The message-related API operations for this bot.</param>
internal sealed record TelegramApi(IMessageApi Messages) : ITelegramApi;
