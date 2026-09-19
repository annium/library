using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Converters;

/// <summary>
/// Converts a Binance error response (<c>{"code": ..., "msg": ...}</c>) into an <see cref="OperationResult"/>.
/// </summary>
public class OperationResultConverter : JsonConverter<OperationResult?>
{
    /// <summary>Reads a Binance error object into an <see cref="OperationResult"/>.</summary>
    /// <param name="reader">The reader positioned at the start of the error object.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The active serializer options.</param>
    /// <returns>The parsed operation result, or <c>null</c> if the <c>code</c> or <c>msg</c> field was missing.</returns>
    public override OperationResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected {JsonTokenType.StartObject}, got {reader.TokenType}");

        var currentDepth = reader.CurrentDepth;

        var code = long.MinValue;
        string? message = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                return code != long.MinValue && message is not null ? new OperationResult(code, message) : null;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    // Binance spells this field both ways. Errors answer with a JSON number; the algo
                    // endpoints answer a successful cancellation with the string "200". Reading only the
                    // number threw on the string, and the throw surfaced as a parse failure on a response
                    // the exchange considered a success - so the tolerance is not politeness, it is the
                    // difference between a cancellation reported as done and one reported as unreadable
                    case "code":
                        code = reader.TokenType switch
                        {
                            JsonTokenType.Number => reader.GetInt64(),
                            JsonTokenType.String
                                when long.TryParse(
                                    reader.GetString(),
                                    NumberStyles.Integer,
                                    CultureInfo.InvariantCulture,
                                    out var parsed
                                ) => parsed,
                            _ => code,
                        };
                        break;
                    case "msg":
                        message = reader.GetString() ?? string.Empty;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of json");
    }

    /// <summary>Not supported; operation results are only ever read from Binance responses, never written.</summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The operation result to write.</param>
    /// <param name="options">The active serializer options.</param>
    public override void Write(Utf8JsonWriter writer, OperationResult? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
