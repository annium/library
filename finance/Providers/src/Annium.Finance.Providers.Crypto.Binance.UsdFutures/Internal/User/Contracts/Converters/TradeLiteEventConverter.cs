using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Serialization.Json;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Converters;

/// <summary>
/// Reads the user data stream's <c>TRADE_LITE</c> event, and returns null for any other event so one
/// dispatch can try each shape in turn.
/// </summary>
/// <remarks>
/// Unlike the order and conditional-order events, this one is flat: the fill's fields sit at the top
/// level beside the event tag rather than under a nested object.
/// </remarks>
internal class TradeLiteEventConverter : JsonConverter<TradeLiteEvent?>
{
    /// <summary>Reads a trade-lite event.</summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the start of the event object.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <returns>The parsed event, or null when the message is a different event or names no symbol.</returns>
    public override TradeLiteEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("deserialization failed");

        var currentDepth = reader.CurrentDepth;

        var isTradeLite = false;
        var symbol = string.Empty;
        var orderId = string.Empty;
        var clientOrderId = string.Empty;
        var tradeId = string.Empty;
        var price = 0m;
        var qty = 0m;
        var isMaker = false;
        var at = 0L;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                if (!isTradeLite || symbol.IsNullOrWhiteSpace() || clientOrderId.IsNullOrWhiteSpace())
                    return default;

                return new TradeLiteEvent(symbol, orderId, clientOrderId, tradeId, price, qty, isMaker, at);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var propertyName = reader.GetString();

            reader.Read();

            switch (propertyName)
            {
                case "e":
                    isTradeLite = reader.GetString() == "TRADE_LITE";
                    break;
                case "s":
                    symbol = reader.GetString();
                    break;
                case "i":
                    orderId = reader.GetInt64().ToString();
                    break;
                case "c":
                    clientOrderId = reader.GetString();
                    break;
                case "t":
                    tradeId = reader.GetInt64().ToString();
                    break;
                // the price and quantity of *this fill*, not of the order: the order's own are `p` and `q`,
                // which are the requested values and are zero on a market order
                case "L":
                    price = reader.GetDecimalFromString();
                    break;
                case "l":
                    qty = reader.GetDecimalFromString();
                    break;
                case "m":
                    isMaker = reader.GetBoolean();
                    break;
                case "E":
                    at = reader.GetInt64();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("deserialization failed");
    }

    /// <summary>Writing is not supported: this contract is read-only.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override void Write(Utf8JsonWriter writer, TradeLiteEvent? value, JsonSerializerOptions options) =>
        throw new NotSupportedException();
}
