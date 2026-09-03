using Annium.Core.Mapper.Attributes;

namespace Annium.Social.Telegram.Integration.Shared.Domain;

/// <summary>
/// The kind of Telegram chat a message or update belongs to.
/// </summary>
[AutoMapped]
public enum ChatType
{
    /// <summary>
    /// A one-on-one conversation between the bot and a single user.
    /// </summary>
    Private,

    /// <summary>
    /// A basic group chat.
    /// </summary>
    Group,

    /// <summary>
    /// A supergroup chat.
    /// </summary>
    SuperGroup,

    /// <summary>
    /// A broadcast channel.
    /// </summary>
    Channel,
}
