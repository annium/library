using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Messages;

namespace Annium.Integrations.Social.Telegram.Internal.Integration;

internal sealed record TelegramApi(IMessageApi Messages) : ITelegramApi;
