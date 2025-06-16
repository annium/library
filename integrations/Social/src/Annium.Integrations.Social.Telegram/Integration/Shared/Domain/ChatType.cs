using Annium.Core.Mapper.Attributes;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

[AutoMapped]
public enum ChatType
{
    Private,
    Group,
    SuperGroup,
    Channel,
}
