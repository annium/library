using System.Net.Mime;
using Annium.Serialization.Abstractions;

namespace Annium.Integrations.Social.Telegram.Internal;

internal static class Constants
{
    public const string ServiceKey = "telegram";
    public static readonly SerializerKey SerializerKey = SerializerKey.Create(
        ServiceKey,
        MediaTypeNames.Application.Json
    );
}
