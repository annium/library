using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.User.Operations;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Cancels every open order on a symbol of the real account, and reports what it found. Run by hand; in the
/// <b>probe</b> block, so no recipe runs it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists before the first trading run rather than after it.</b> A trading stage that fails
/// part-way does not clean up after itself: the fixture's teardown runs at the end of a passing test, and
/// an assertion that throws, or a runner that cancels the job, leaves whatever the stage placed sitting on
/// a real account. The recovery is then done by hand, under exactly the conditions - something has just
/// gone wrong, and it is not yet clear what - in which writing a tool is the worst time to write one.
/// </para>
/// <para>
/// <b>It cancels orders and does nothing else.</b> That is the whole of what a cash account can be left
/// dirty with by this suite, because nothing in the suite fills. If a future stage is ever allowed to fill,
/// what it bought is not this tool's to sell back: that is a decision for whoever ran it.
/// </para>
/// <para>
/// To run it: <c>dotnet test ... --filter-trait "block=probe" --filter-method "*AccountCleanupTool*"</c>,
/// or from an IDE. It asserts nothing about the code and everything it reports is in its log.
/// </para>
/// </remarks>
[Trait(TestBlock.Name, TestBlock.Probe)]
public class AccountCleanupTool : ProvidersTestBase
{
    /// <summary>The symbol to clear.</summary>
    private const string Symbol = "DOTUSDT";

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountCleanupTool"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AccountCleanupTool(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider, with the scheduled reload slow enough to fit the weight budget.
    /// The tool is run after something has gone wrong, which is when the budget is least likely to be free.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot(
            new ProviderConfiguration
            {
                ReloadContext = new CompositeLoaderConfig(200, 5, 1000, 5_000, 100),
                ReloadOrders = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
                ReloadTrades = new CompositeLoaderConfig(200, 5, 1000, 15_000, 100),
            }
        );
    }

    /// <summary>
    /// Connects, reports every open order on the symbol, cancels them all, and reports what is left.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.WriteTimeoutMs)]
    public async Task CancelEveryOpenOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var orders = new ConcurrentQueue<OrderModel>();
        var assets = new ConcurrentQueue<AssetModel>();

        this.Trace("get user connector");
        var factory = Get<IUserConnectorFactory>();
        await using var connector = factory.Create(Settings.User);

        using var assetSubscription = connector.Assets.Subscribe(x =>
        {
            if (x.Type is ChangeEventType.Init)
                foreach (var item in x.Items)
                    assets.Enqueue(item);
            else
                assets.Enqueue(x.Item);
        });

        using var subscription = connector.Orders.Subscribe(x =>
        {
            if (x.Type is ChangeEventType.Init)
                foreach (var item in x.Items)
                    orders.Enqueue(item);
            else
                orders.Enqueue(x.Item);
        });

        this.Trace("await until user connector is ready");
        await connector.WhenConnectedAsync(ct);

        // a moment for the opening snapshot to land, so what is reported below is the account and not an
        // empty queue read too early
        await Task.Delay(2000, ct);

        // the account as it stands, which is the other half of what this tool is for: after something has
        // gone wrong, "what is open" and "what is held" are the two questions, and only one of them is
        // answered by cancelling
        var balances = assets
            .ToArray()
            .GroupBy(x => x.Resource)
            .Select(g => g.Last())
            .Where(x => x.Free + x.Locked > 0m)
            .ToArray();

        var open = orders
            .Where(x => x.Symbol == Symbol && x.Status is OrderStatus.New or OrderStatus.PartiallyFilled)
            .DistinctBy(x => x.Id)
            .ToArray();

        this.Trace<string, string>("found {count} open order(s) on {symbol}", open.Length.ToString(), Symbol);
        foreach (var order in open)
            this.Trace<string, string>("open: {id} {order}", order.Id, order.ToString());

        if (open.Length == 0)
        {
            // the venue refuses a cancel-all on a symbol with nothing open - measured 2026-09-21, HTTP 400
            // under the code its reference calls CANCEL_REJECTED. That code is a family rather than a
            // reason, carrying "market is closed" and "this account may not place or cancel orders" too,
            // so it cannot be read as success. Nothing to do here is not a failure either
            this.Trace<string>("nothing open on {symbol}, nothing to cancel", Symbol);
        }
        else
        {
            this.Trace<string>("cancel all orders on {symbol}", Symbol);
            await connector.CancelAllOrdersAsync(Symbol).UnwrapAsync().WaitAsync(ct);
        }

        await Task.Delay(2000, ct);

        var left = orders
            .Where(x => x.Symbol == Symbol && x.Status is OrderStatus.New or OrderStatus.PartiallyFilled)
            .DistinctBy(x => x.Id)
            .Where(x => open.All(o => o.Id != x.Id))
            .ToArray();

        this.Trace<string>("{count} order(s) appeared after the cancellation", left.Length.ToString());

        // written to a file rather than only logged, because a passing test's log is not shown: a tool
        // that reports by logging runs, passes, and tells nobody anything - which is the failure mode this
        // whole family of tools exists to avoid
        var report = new StringBuilder()
            .AppendLine($"account report for {Symbol} at {DateTime.UtcNow:O}")
            .AppendLine($"balances ({balances.Length}):");
        foreach (var asset in balances)
            report.AppendLine($"  {asset.Resource}: free={asset.Free} locked={asset.Locked}");

        report.AppendLine($"open orders on {Symbol} before cancelling ({open.Length}):");
        foreach (var order in open)
            report.AppendLine($"  {order.Id} {order.Side} {order.TotalQty} @ {order.Price} [{order.Status}]");

        report.AppendLine($"orders appearing after the cancellation: {left.Length}");

        var path = Path.Combine(Path.GetTempPath(), $"annium-account-report-{Symbol}.txt");
        await File.WriteAllTextAsync(path, report.ToString(), ct);
        this.Warn<string>("account report written to {path}", path);

        // the tool's one assertion: it is reporting the account, and an account it could not read is not an
        // account it has cleared
        connector.Status.Is(ConnectorStatus.Connected);
    }
}
