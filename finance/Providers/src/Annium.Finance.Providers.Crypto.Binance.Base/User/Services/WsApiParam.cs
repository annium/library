using System.Globalization;

namespace Annium.Finance.Providers.Crypto.Binance.Base.User.Services;

/// <summary>
/// One parameter of a WebSocket API request, carrying both how it is signed and how it is serialized.
/// </summary>
/// <remarks>
/// The two are not the same, which is the whole reason this type exists rather than a plain string. The
/// signature is computed over a flat text payload where every value is its text and nothing is quoted;
/// the request itself is JSON, where a number and a string are different values. A builder that knew only
/// the text would have to guess which of the two each parameter is, and a builder that knew only the JSON
/// value would have to render it back to text to sign it - both of which produce a request whose signature
/// does not match its own body, and the exchange answers that with a refusal that names neither.
/// </remarks>
/// <param name="Value">The value as it appears in the signature payload.</param>
/// <param name="IsNumber">Whether the value is serialized as a JSON number rather than a JSON string.</param>
public readonly record struct WsApiParam(string Value, bool IsNumber)
{
    /// <summary>Creates a parameter serialized as a JSON string.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The parameter.</returns>
    public static WsApiParam Text(string value) => new(value, false);

    /// <summary>Creates a parameter serialized as a JSON number.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The parameter.</returns>
    public static WsApiParam Number(long value) => new(value.ToString(CultureInfo.InvariantCulture), true);
}
