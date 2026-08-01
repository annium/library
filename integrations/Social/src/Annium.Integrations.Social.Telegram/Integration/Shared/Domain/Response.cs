using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

/// <summary>
/// The Telegram Bot API's generic response envelope: either a successful result, or a failure description.
/// </summary>
/// <typeparam name="T">The type of the successful result payload.</typeparam>
public sealed record Response<T>
{
    /// <summary>
    /// Whether the API call succeeded; when <see langword="true"/>, <see cref="Result"/> is populated, otherwise
    /// <see cref="Description"/> is.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(Description))]
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    /// <summary>
    /// The successful result payload, populated only when <see cref="Ok"/> is <see langword="true"/>.
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; init; }

    /// <summary>
    /// The human-readable failure description, populated only when <see cref="Ok"/> is <see langword="false"/>.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
