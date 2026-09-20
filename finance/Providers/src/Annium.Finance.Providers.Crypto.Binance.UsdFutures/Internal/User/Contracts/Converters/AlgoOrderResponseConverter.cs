using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;
using Annium.Linq;
using Annium.Serialization.Json;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Converters;

/// <summary>
/// Reads a conditional ("algo") order — from the placement, open-orders or history endpoints alike — into an
/// <see cref="OrderModel"/>, and drops the records an ordinary order already accounts for.
/// </summary>
/// <remarks>
/// <para>
/// One converter for three response shapes. The endpoints differ only in which extra fields they add
/// (<c>actualQty</c> and <c>isActivated</c> on the open list; <c>actualPrice</c>, <c>actualType</c> and
/// <c>tpOrderType</c> on the history), and a reader that names the fields it wants and skips the rest does
/// not care which it is looking at.
/// </para>
/// <para>
/// The field names are not the ordinary order endpoint's, and the differences are the sort that read as
/// defects later: <c>orderType</c> where that one says <c>type</c>, <c>algoStatus</c> where it says
/// <c>status</c>, <c>triggerPrice</c> where it says <c>stopPrice</c>, and <c>algoId</c> as a JSON
/// <em>number</em> where <c>orderId</c> is one too but is read into a string the same way.
/// </para>
/// <para>
/// <strong>A triggered record is dropped.</strong> When the exchange fires a conditional order it creates an
/// ordinary order carrying the same client id, so a <c>TRIGGERED</c> or <c>FINISHED</c> algo record is a
/// second view of an order the caller already has - with a worse account of it, since the algo record cannot
/// say whether the book filled or cancelled it. Returning null here is what keeps one order from appearing
/// twice; the caller filters nulls out of the collection, exactly as it does for assets with no margin.
/// </para>
/// </remarks>
internal class AlgoOrderResponseConverter : JsonConverter<OrderModel?>
{
    /// <summary>
    /// Reads one conditional order.
    /// </summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the start of the object.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <returns>
    /// The parsed order; null when the record is missing its ids or symbol, or when its status says an
    /// ordinary order supersedes it.
    /// </returns>
    public override OrderModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("deserialization failed");

        var currentDepth = reader.CurrentDepth;

        var id = string.Empty;
        var clientOrderId = string.Empty;
        var range = OrientationRange.Both;
        var symbol = string.Empty;
        var type = default(OrderType);
        var side = default(OrderSide);
        var totalQty = 0m;
        var price = 0m;
        var levelPrice = 0m;
        var reduceOnly = false;
        var algoStatus = string.Empty;
        var executedQty = 0m;
        var executedPrice = 0m;
        var createdAt = 0L;
        var updatedAt = 0L;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                if (id.IsNullOrWhiteSpace() || clientOrderId.IsNullOrWhiteSpace() || symbol.IsNullOrWhiteSpace())
                    return default;

                // superseded, or a status this venue has not shown us before: either way the caller is
                // better served by the ordinary order than by a guess made here
                if (!AlgoStatuses.TryMap(algoStatus, out var status))
                    return default;

                return new OrderModel(
                    id,
                    clientOrderId,
                    range,
                    symbol,
                    side,
                    type,
                    totalQty,
                    price,
                    levelPrice,
                    reduceOnly,
                    createdAt,
                    status,
                    executedQty,
                    executedPrice,
                    updatedAt
                );
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "algoId":
                        id = reader.GetInt64().ToString();
                        break;
                    case "clientAlgoId":
                        clientOrderId = reader.GetString();
                        break;
                    case "positionSide":
                        range = OrientationRanges.StringToValue.MapValue(reader.GetString());
                        break;
                    case "symbol":
                        symbol = reader.GetString();
                        break;
                    case "orderType":
                        type = OrderTypes.StringToValue.MapValue(reader.GetString());
                        break;
                    case "side":
                        side = OrderSides.StringToValue.MapValue(reader.GetString());
                        break;
                    case "quantity":
                        totalQty = reader.GetDecimalFromString();
                        break;
                    case "price":
                        price = reader.GetDecimalFromString();
                        break;
                    case "triggerPrice":
                        levelPrice = reader.GetDecimalFromString();
                        break;
                    case "reduceOnly":
                        reduceOnly = reader.GetBoolean();
                        break;
                    case "algoStatus":
                        algoStatus = reader.GetString() ?? string.Empty;
                        break;
                    // present only once a trigger has fired, and read for completeness: a record carrying
                    // them is dropped above, so these never reach a caller through this converter
                    case "actualQty":
                        executedQty = reader.GetDecimalFromString();
                        break;
                    case "actualPrice":
                        executedPrice = reader.GetDecimalFromString();
                        break;
                    case "createTime":
                        createdAt = reader.GetInt64();
                        break;
                    case "updateTime":
                        updatedAt = reader.GetInt64();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        throw new JsonException("deserialization failed");
    }

    /// <summary>Writing is not supported: this contract is read-only.</summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options in effect.</param>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override void Write(Utf8JsonWriter writer, OrderModel? value, JsonSerializerOptions options) =>
        throw new NotSupportedException();
}
