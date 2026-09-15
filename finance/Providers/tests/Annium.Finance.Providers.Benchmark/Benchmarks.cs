using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Domain.Market.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using BenchmarkDotNet.Attributes;

namespace Annium.Finance.Providers.Benchmark;

/// <summary>
/// Measures what one provider operation's outcome costs: the result itself, and the task that carries it
/// back to the caller.
/// </summary>
/// <remarks>
/// <para>
/// Every provider and connector call produces one of these, and the backtest produces one per order per
/// tick — so this is paid per operation, and it was paid twice: once for the result object and once for the
/// <c>Task</c> wrapping it. The rows below separate those two costs, because they are removed by two
/// different changes and either can be reverted without the other.
/// </para>
/// <para>
/// The <c>Deliver_</c> rows are the ones that answer the question. A connector command whose work is already
/// done — the backtest's matching engine, a client-side validation refusal — returns through them, and what
/// they cost is what the shape of the return type costs, with no exchange in the way.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Benchmarks
{
    /// <summary>An order to carry, built once so its own allocation is not charged to the measurement.</summary>
    private static readonly OrderModel _order = new(
        "6789",
        "client-1",
        OrientationRange.Both,
        "BTCUSDT",
        OrderSide.Buy,
        OrderType.Limit,
        1.5m,
        65000m,
        0m,
        false,
        1_757_000_000_000,
        OrderStatus.New,
        0m,
        0m,
        1_757_000_000_000
    );

    /// <summary>A successful outcome carrying nothing — what a cancellation answers with.</summary>
    /// <returns>The result.</returns>
    /// <remarks>
    /// No baseline is declared on this set. Every construction row is meant to reach zero, and a ratio
    /// against a zero baseline prints as <c>?</c> for every row — the allocation column is what these rows
    /// are read for, and a timing near zero is itself the finding.
    /// </remarks>
    [Benchmark]
    public UserResult CreateUser() => UserResult.Ok();

    /// <summary>A successful outcome carrying an order — what a placement answers with.</summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public UserResult<OrderModel?> CreateUserWithData() => UserResult.Ok<OrderModel?>(_order);

    /// <summary>A refusal carrying a message, which is the path that also builds a string.</summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public UserResult CreateUserFailure() => UserResult.New(UserOperationStatus.BadRequest, "rejected");

    /// <summary>The market half of the pair, yielded once per page by a candle fetch.</summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public MarketResult<string?> CreateMarketWithData() => MarketResult.Ok<string?>("payload");

    /// <summary>Reading the outcome of a successful result, which is what every caller does with one.</summary>
    /// <returns>Whether it succeeded.</returns>
    [Benchmark]
    public bool CreateAndCheck() => UserResult.Ok<OrderModel?>(_order).IsSuccess;

    /// <summary>
    /// Carrying a refusal forward onto a differently-shaped result — the path a validation failure takes on
    /// its way to the caller.
    /// </summary>
    /// <returns>The carried result.</returns>
    [Benchmark]
    public UserResult<OrderModel?> CarryForward() =>
        UserResult.From<OrderModel?>(UserResult.New(UserOperationStatus.BadRequest, "rejected"), null);

    /// <summary>
    /// A command that answers without going anywhere, delivered as the interface declares it.
    /// </summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public ValueTask<UserResult<OrderModel?>> DeliverValueTaskAsync() =>
        ValueTask.FromResult(UserResult.Ok<OrderModel?>(_order));

    /// <summary>
    /// The same delivery through a <c>Task</c>, which is what the interface declared before.
    /// </summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public Task<UserResult<OrderModel?>> DeliverTaskAsync() => Task.FromResult(UserResult.Ok<OrderModel?>(_order));

    /// <summary>
    /// The delivery as a caller actually consumes it: awaited. An awaited result that is already there is
    /// the whole point of the value-task shape, and the row exists so a regression to <c>Task</c> shows up
    /// here rather than only in a backtest.
    /// </summary>
    /// <returns>Whether it succeeded.</returns>
    [Benchmark]
    public async ValueTask<bool> DeliverAndAwaitAsync()
    {
        var result = await ValueTask.FromResult(UserResult.Ok<OrderModel?>(_order));

        return result.IsSuccess;
    }
}
