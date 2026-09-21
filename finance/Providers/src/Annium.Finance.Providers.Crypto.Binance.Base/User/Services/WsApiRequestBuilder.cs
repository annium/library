using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Annium.Finance.Providers.Crypto.Binance.Base.User.Services;

/// <summary>
/// Builds request frames for Binance's WebSocket API, including the signature a signed method requires.
/// </summary>
/// <remarks>
/// <para>
/// The WebSocket API is a request/response protocol: a frame carries an <c>id</c> the reply is matched by, a
/// <c>method</c>, and a <c>params</c> object. Signing it is close to signing a REST request and differs in
/// two ways that are easy to miss and impossible to see afterwards, because a wrong signature comes back as
/// a refusal that names no parameter.
/// </para>
/// <para>
/// First, the payload is built from the parameters <b>sorted by name</b> rather than in the order they were
/// added - a REST query is signed exactly as it was composed. Second, the values go in <b>raw</b>: the
/// documented payload for a request whose symbol is non-ASCII carries those characters themselves, so
/// percent-encoding the value here produces a signature over a string the exchange never reconstructs.
/// </para>
/// </remarks>
public static class WsApiRequestBuilder
{
    /// <summary>
    /// Builds a signed request frame.
    /// </summary>
    /// <remarks>
    /// <paramref name="parameters"/> supplies the method's own arguments; the account's key and the
    /// timestamp are added here, because every signed method takes them and a caller that had to remember
    /// them would eventually not.
    /// </remarks>
    /// <param name="id">The request id the reply will carry back.</param>
    /// <param name="method">The method being called.</param>
    /// <param name="signatureService">Supplies the API key, the server time and the signature.</param>
    /// <param name="parameters">The method's own parameters, if any.</param>
    /// <returns>The frame, as UTF-8 bytes ready to send.</returns>
    public static ReadOnlyMemory<byte> BuildSigned(
        string id,
        string method,
        ISignatureService signatureService,
        IReadOnlyDictionary<string, WsApiParam>? parameters = null
    )
    {
        // ordinal rather than culture-aware: the payload is bytes the exchange re-derives, so the ordering
        // must not depend on where this runs. Every parameter name this API takes is lowerCamelCase, where
        // ordinal and alphabetical agree - which the documented golden values pin rather than assume
        var signed = new SortedDictionary<string, WsApiParam>(StringComparer.Ordinal);

        if (parameters is not null)
            foreach (var (name, value) in parameters)
                signed[name] = value;

        signed["apiKey"] = WsApiParam.Text(signatureService.GetKey());
        signed["timestamp"] = WsApiParam.Number(signatureService.ServerTime);

        var payload = BuildSignaturePayload(signed);
        signed["signature"] = WsApiParam.Text(signatureService.GetSignature(payload));

        return Build(id, method, signed);
    }

    /// <summary>
    /// Builds an unsigned request frame.
    /// </summary>
    /// <param name="id">The request id the reply will carry back.</param>
    /// <param name="method">The method being called.</param>
    /// <param name="parameters">The method's parameters, if any.</param>
    /// <returns>The frame, as UTF-8 bytes ready to send.</returns>
    public static ReadOnlyMemory<byte> Build(
        string id,
        string method,
        IReadOnlyDictionary<string, WsApiParam>? parameters = null
    )
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        writer.WriteStartObject();
        writer.WriteString("id", id);
        writer.WriteString("method", method);

        // omitted entirely when there are none: the two methods that take no parameters are documented
        // with no params member at all, and an empty object is a different frame from an absent one
        if (parameters is { Count: > 0 })
        {
            writer.WriteStartObject("params");
            foreach (var (name, param) in parameters)
            {
                writer.WritePropertyName(name);
                if (param.IsNumber)
                    writer.WriteRawValue(param.Value);
                else
                    writer.WriteStringValue(param.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
        writer.Flush();

        return buffer.WrittenMemory;
    }

    /// <summary>
    /// Builds the flat text the signature is computed over.
    /// </summary>
    /// <param name="parameters">The parameters, already sorted by name and without a signature.</param>
    /// <returns>The signature payload.</returns>
    private static string BuildSignaturePayload(SortedDictionary<string, WsApiParam> parameters)
    {
        var payload = new StringBuilder();

        foreach (var (name, param) in parameters)
        {
            if (payload.Length > 0)
                payload.Append('&');

            payload.Append(name).Append('=').Append(param.Value);
        }

        return payload.ToString();
    }
}
