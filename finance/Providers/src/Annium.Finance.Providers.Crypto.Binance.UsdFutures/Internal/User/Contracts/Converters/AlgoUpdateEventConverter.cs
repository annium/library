using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Linq;
using Annium.Serialization.Json;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Converters;

/// <summary>
/// Reads the user data stream's <c>ALGO_UPDATE</c> event into an <see cref="AlgoUpdateEvent"/>, and returns
/// null for any other event so the stream's single dispatch can try each shape in turn.
/// </summary>
/// <remarks>
/// <para>
/// <strong>The payload nests a field under its own name.</strong> The event's order object is <c>o</c>, and
/// inside that object the order's type is also <c>o</c>. A reader that matches on the property name without
/// tracking depth will read the type as the object or the object as the type, and either way will do it
/// silently. Depth is what separates them here.
/// </para>
/// <para>
/// Every field name below was read off a real message captured from the live stream on 2026-09-19, because
/// the exchange documents this payload only through a schema component that cannot be fetched.
/// </para>
/// </remarks>
internal class AlgoUpdateEventConverter : JsonConverter<AlgoUpdateEvent?>
{
    /// <summary>Reads an algo update event.</summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the start of the event object.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <returns>The parsed event, or null when the message is a different event or is missing its ids.</returns>
    public override AlgoUpdateEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("deserialization failed");

        var rootDepth = reader.CurrentDepth;

        var isAlgoUpdate = false;
        var algoId = string.Empty;
        var clientAlgoId = string.Empty;
        var range = OrientationRange.Both;
        var symbol = string.Empty;
        var type = default(OrderType);
        var side = default(OrderSide);
        var totalQty = 0m;
        var price = 0m;
        var levelPrice = 0m;
        var reduceOnly = false;
        var algoStatus = string.Empty;
        var updatedAt = 0L;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == rootDepth)
            {
                if (
                    !isAlgoUpdate
                    || algoId.IsNullOrWhiteSpace()
                    || clientAlgoId.IsNullOrWhiteSpace()
                    || symbol.IsNullOrWhiteSpace()
                )
                    return default;

                var isSuperseded = !AlgoStatuses.TryMap(algoStatus, out var status);

                return new AlgoUpdateEvent(
                    algoId,
                    clientAlgoId,
                    range,
                    symbol,
                    type,
                    side,
                    totalQty,
                    price,
                    levelPrice,
                    reduceOnly,
                    status,
                    isSuperseded,
                    updatedAt
                );
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var propertyName = reader.GetString();
            // the depth the name was read at, which is the only thing telling the event's order object
            // apart from the order type nested inside it - both are spelled "o"
            var atRoot = reader.CurrentDepth == rootDepth + 1;

            reader.Read();

            if (atRoot)
            {
                switch (propertyName)
                {
                    case "e":
                        isAlgoUpdate = reader.GetString() == "ALGO_UPDATE";
                        break;
                    case "E":
                        updatedAt = reader.GetInt64();
                        break;
                    case "o":
                        // the order object: fall through into it rather than skipping, so its fields are
                        // read by the arms below at their own depth
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                continue;
            }

            switch (propertyName)
            {
                case "aid":
                    algoId = reader.GetInt64().ToString();
                    break;
                case "caid":
                    clientAlgoId = reader.GetString();
                    break;
                case "ps":
                    range = OrientationRanges.StringToValue.MapValue(reader.GetString());
                    break;
                case "s":
                    symbol = reader.GetString();
                    break;
                case "o":
                    type = OrderTypes.StringToValue.MapValue(reader.GetString());
                    break;
                case "S":
                    side = OrderSides.StringToValue.MapValue(reader.GetString());
                    break;
                case "q":
                    totalQty = reader.GetDecimalFromString();
                    break;
                case "p":
                    price = reader.GetDecimalFromString();
                    break;
                case "tp":
                    levelPrice = reader.GetDecimalFromString();
                    break;
                case "R":
                    reduceOnly = reader.GetBoolean();
                    break;
                case "X":
                    algoStatus = reader.GetString() ?? string.Empty;
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
    public override void Write(Utf8JsonWriter writer, AlgoUpdateEvent? value, JsonSerializerOptions options) =>
        throw new NotSupportedException();
}
