using System.Collections.Generic;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User.Requests;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Contracts.Domain;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.Services;

/// <summary>
/// Builds the query parameters for the order management endpoints (place, modify, cancel, cancel-all) from the
/// library's generic order requests, translating enums to their Binance wire values along the way.
/// </summary>
internal class QueryProcessor
{
    /// <summary>
    /// Builds the query for placing a new order, adding the type-specific parameters (<c>timeInForce</c>,
    /// <c>price</c>, <c>stopPrice</c>) required by each order type. <c>reduceOnly</c> is only sent in one-way
    /// mode (<see cref="OrientationRange.Both"/>), since Binance rejects it together with an explicit
    /// <c>positionSide</c> in hedge mode.
    /// </summary>
    /// <param name="request">The order placement parameters.</param>
    /// <returns>A result carrying the query parameters, or a validation failure if the request is invalid.</returns>
    public UserResult<Dictionary<string, string>> BuildInitOrderQuery(IInitOrderRequest request)
    {
        var validationResult = request.Validate();
        if (validationResult.IsFailure)
        {
            return UserResult.From(validationResult, new Dictionary<string, string>());
        }

        var result = new Dictionary<string, string>();
        result["newClientOrderId"] = request.Id;
        result["symbol"] = request.Symbol;
        result["side"] = OrderSides.ValueToString[request.Side];
        result["positionSide"] = OrientationRanges.ValueToString[request.Range];
        result["type"] = OrderTypes.ValueToString[request.Type];
        result["newOrderRespType"] = "RESULT";

        switch (request.Type)
        {
            case OrderType.Limit:
                result["timeInForce"] = "GTC";
                result["quantity"] = request.Qty.ToGeneralInvariantString();
                result["price"] = request.Price.ToGeneralInvariantString();
                TrySetReduceOnly(request, result);
                break;
            case OrderType.Market:
                result["quantity"] = request.Qty.ToGeneralInvariantString();
                TrySetReduceOnly(request, result);
                break;
            case OrderType.StopLossMarket:
                result["quantity"] = request.Qty.ToGeneralInvariantString();
                TrySetReduceOnly(request, result);
                result["stopPrice"] = request.LevelPrice.ToGeneralInvariantString();
                break;
            case OrderType.TakeProfitMarket:
                result["quantity"] = request.Qty.ToGeneralInvariantString();
                TrySetReduceOnly(request, result);
                result["stopPrice"] = request.LevelPrice.ToGeneralInvariantString();
                break;
            case OrderType.StopLossLimit:
                result["timeInForce"] = "GTC";
                result["quantity"] = request.Qty.ToGeneralInvariantString();
                result["price"] = request.Price.ToGeneralInvariantString();
                result["stopPrice"] = request.LevelPrice.ToGeneralInvariantString();
                TrySetReduceOnly(request, result);
                break;
            case OrderType.TakeProfitLimit:
                result["timeInForce"] = "GTC";
                result["quantity"] = request.Qty.ToGeneralInvariantString();
                result["price"] = request.Price.ToGeneralInvariantString();
                result["stopPrice"] = request.LevelPrice.ToGeneralInvariantString();
                TrySetReduceOnly(request, result);
                break;
        }

        return UserResult.Ok(result);

        static void TrySetReduceOnly(IInitOrderRequest request, IDictionary<string, string> result)
        {
            if (request.ReduceOnly && request.Range is OrientationRange.Both)
                result["reduceOnly"] = "true";
        }
    }

    /// <summary>
    /// Whether an order type is a conditional one, and so belongs to the algo endpoints rather than the
    /// ordinary order endpoint.
    /// </summary>
    /// <remarks>
    /// The single place that answers this question. It decides which endpoint a placement goes to, which
    /// endpoint a cancellation goes to, and which store a history load has to read - and if those three ever
    /// disagreed, an order would be placed in one store and looked for in another. The four types moved on
    /// 2025-12-09; the ordinary endpoint refuses them with <c>-4120</c>.
    /// </remarks>
    /// <param name="type">The order type.</param>
    /// <returns><see langword="true"/> when the type is conditional.</returns>
    public static bool IsConditional(OrderType type) =>
        type
            is OrderType.StopLossMarket
                or OrderType.TakeProfitMarket
                or OrderType.StopLossLimit
                or OrderType.TakeProfitLimit;

    /// <summary>
    /// Builds the query for placing a conditional order through <c>POST /fapi/v1/algoOrder</c>.
    /// </summary>
    /// <remarks>
    /// Not the ordinary placement query with a different path: four parameter names differ. The client id is
    /// <c>clientAlgoId</c>, the trigger is <c>triggerPrice</c> rather than <c>stopPrice</c>, and
    /// <c>algoType</c> has no counterpart at all and is required. Sending the ordinary names here is
    /// accepted by nothing and refused unhelpfully, which is why these are built rather than translated.
    /// </remarks>
    /// <param name="request">The order to place.</param>
    /// <returns>A result carrying the query parameters, or the validation failure that stopped it.</returns>
    public UserResult<Dictionary<string, string>> BuildInitAlgoOrderQuery(IInitOrderRequest request)
    {
        var validationResult = request.Validate();
        if (validationResult.IsFailure)
        {
            return UserResult.From(validationResult, new Dictionary<string, string>());
        }

        if (!IsConditional(request.Type))
        {
            return UserResult.New(
                UserOperationStatus.BadRequest,
                new Dictionary<string, string>(),
                $"{request.Type} is not a conditional order type and does not belong on the algo endpoint"
            );
        }

        var result = new Dictionary<string, string>
        {
            ["algoType"] = "CONDITIONAL",
            ["clientAlgoId"] = request.Id,
            ["symbol"] = request.Symbol,
            ["side"] = OrderSides.ValueToString[request.Side],
            ["positionSide"] = OrientationRanges.ValueToString[request.Range],
            ["type"] = OrderTypes.ValueToString[request.Type],
            ["quantity"] = request.Qty.ToGeneralInvariantString(),
            ["triggerPrice"] = request.LevelPrice.ToGeneralInvariantString(),
            ["newOrderRespType"] = "RESULT",
        };

        // the limit-flavoured conditionals carry the price the order takes once it triggers; the market ones
        // have none, and sending a price with them is how a STOP_MARKET quietly becomes a STOP
        if (request.Type is OrderType.StopLossLimit or OrderType.TakeProfitLimit)
        {
            result["timeInForce"] = "GTC";
            result["price"] = request.Price.ToGeneralInvariantString();
        }

        if (request.ReduceOnly && request.Range is OrientationRange.Both)
            result["reduceOnly"] = "true";

        return UserResult.Ok(result);
    }

    /// <summary>
    /// Builds the query for modifying an existing order. Binance's amend endpoint only supports limit orders, so
    /// any other order type is rejected up front.
    /// </summary>
    /// <param name="request">The modification parameters, including the order being modified.</param>
    /// <returns>A result carrying the query parameters, or a bad-request failure if the order is not a limit order.</returns>
    public UserResult<Dictionary<string, string>> BuildModifyOrderQuery(IModifyOrderRequest request)
    {
        var result = new Dictionary<string, string>();

        if (request.Type is not OrderType.Limit)
        {
            return UserResult.New(UserOperationStatus.BadRequest, result, "Only limit orders are supported");
        }

        result["origClientOrderId"] = request.Order.ClientOrderId;
        result["symbol"] = request.Order.Symbol;
        result["side"] = OrderSides.ValueToString[request.Side];
        result["quantity"] = request.Qty.ToGeneralInvariantString();
        result["price"] = request.Price.ToGeneralInvariantString();

        return UserResult.Ok(result);
    }

    /// <summary>
    /// Builds the query for canceling an order, identifying it by exchange order id and/or client order id.
    /// </summary>
    /// <param name="request">Identifies the order to cancel.</param>
    /// <returns>A result carrying the query parameters, or a bad-request failure if neither id is specified.</returns>
    public UserResult<Dictionary<string, string>> BuildCancelOrderQuery(ICancelOrderRequest request)
    {
        var result = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Id))
            result["orderId"] = request.Id;

        if (!string.IsNullOrWhiteSpace(request.ClientOrderId))
        {
            result["origClientOrderId"] = request.ClientOrderId;
            result["newClientOrderId"] = request.ClientOrderId;
        }

        if (result.Count == 0)
            return UserResult.New(
                UserOperationStatus.BadRequest,
                result,
                "Either order id or client order id must be specified"
            );

        result["symbol"] = request.Symbol;

        return UserResult.Ok(result);
    }

    /// <summary>
    /// Builds the query for canceling all open orders on a symbol.
    /// </summary>
    /// <param name="symbol">The instrument symbol to cancel orders for.</param>
    /// <returns>A result carrying the query parameters.</returns>
    public UserResult<Dictionary<string, string>> BuildCancelAllOrdersQuery(string symbol)
    {
        var result = new Dictionary<string, string>();

        result["symbol"] = symbol;

        return UserResult.Ok(result);
    }
}
