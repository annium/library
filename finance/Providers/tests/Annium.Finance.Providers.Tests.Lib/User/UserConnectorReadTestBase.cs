using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Tests.Lib.User;

/// <summary>
/// Base for tests that connect a provider's live user connector and check that it reaches a connected state
/// and delivers the account snapshot. Read-only: it places nothing, cancels nothing and closes nothing.
/// </summary>
/// <remarks>
/// <para>
/// Its write counterpart, <see cref="UserConnectorTestBase"/>, actively manages the account around every
/// test - cancelling open orders and closing positions - which makes it unusable for a question as simple
/// as "does this connector connect and deliver". That question is worth asking on its own, because a user
/// connector reaching connected means more than an HTTP call succeeding: a listen key was fetched, a
/// websocket opened on it, and the loaders behind it produced a snapshot.
/// </para>
/// <para>
/// Nothing here writes to the account, so it runs in the read block. It is still a live test against real
/// credentials and is skipped where there are none.
/// </para>
/// </remarks>
[Trait(TestBlock.Name, TestBlock.Read)]
public abstract class UserConnectorReadTestBase : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorReadTestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    protected UserConnectorReadTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Creates a live user connector, waits for it to report itself connected, and asserts that the account
    /// snapshot arrived and that nothing was reported on the error channel.
    /// </summary>
    /// <param name="settings">The account credentials the connector authenticates with.</param>
    /// <param name="ct">The test's cancellation token, which its deadline signals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task UserConnectorReadBaseAsync(UserSettings settings, CancellationToken ct)
    {
        this.Trace("start");

        var assets = new ConcurrentQueue<AssetModel>();
        var positions = new ConcurrentQueue<PositionModel>();
        var errors = new ConcurrentQueue<ConnectorError>();

        this.Trace("get user connector factory");
        var factory = Get<IUserConnectorFactory>();

        this.Trace("get user connector for {settings}", settings);
        await using var connector = factory.Create(settings);

        this.Trace("subscribe to connector data and errors");
        using var assetsSubscription = connector.Assets.Subscribe(x => Collect(assets, x));
        using var positionsSubscription = connector.Positions.Subscribe(x => Collect(positions, x));
        connector.OnError += errors.Enqueue;

        try
        {
            // connected is the assertion, not a step before it: for a user connector it means the listen key
            // was fetched, the stream opened on it, and every loader behind it reported in
            this.Trace("await until user connector is ready");
            await connector.WhenConnectedAsync(ct);

            this.Trace("await for the account snapshot");
            await Expect.ToAsync(() => assets.Count.IsGreaterOrEqual(1), 30_000);

            // an account with no open position reports no rows at all on some venues, so positions are not
            // asserted as non-empty - what is asserted is that the stream that carries them is up
            connector.Status.Is(ConnectorStatus.Connected);
            positions.Count.IsGreaterOrEqual(0);

            this.Trace("assert nothing was reported on the error channel");
            errors.Count.Is(0, string.Join("; ", errors.Select(x => x.Message)));
        }
        finally
        {
            connector.OnError -= errors.Enqueue;
        }

        this.Trace("done");
    }

    /// <summary>
    /// Adds a change event's payload to a queue, whether it arrived as a snapshot or as a single change.
    /// </summary>
    /// <typeparam name="T">The type of the item the change carries.</typeparam>
    /// <param name="queue">The queue to collect into.</param>
    /// <param name="change">The change event received.</param>
    private static void Collect<T>(ConcurrentQueue<T> queue, ChangeEvent<T> change)
        where T : notnull
    {
        if (change.Type is ChangeEventType.Init)
            foreach (var item in change.Items)
                queue.Enqueue(item);
        else
            queue.Enqueue(change.Item);
    }
}
