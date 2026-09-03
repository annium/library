using System.Net.Mime;
using Annium.Serialization.Abstractions;

namespace Annium.Integrations.Social.Telegram.Internal;

/// <summary>
/// Shared keys used to register and resolve this integration's HTTP request factory and serializer in DI.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The key this integration registers its HTTP request factory and serializer under.
    /// </summary>
    public const string ServiceKey = "telegram";

    /// <summary>
    /// The serializer key (service key + JSON content type) used to resolve the Telegram API's JSON serializer.
    /// </summary>
    public static readonly SerializerKey SerializerKey = SerializerKey.Create(
        ServiceKey,
        MediaTypeNames.Application.Json
    );
}
