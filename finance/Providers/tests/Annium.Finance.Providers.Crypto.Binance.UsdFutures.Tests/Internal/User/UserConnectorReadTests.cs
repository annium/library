using System;
using System.Threading.Tasks;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.User;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Connects the live USD-M futures user connector and checks that it reaches connected and delivers the
/// account snapshot. Places nothing, cancels nothing, closes nothing.
/// </summary>
/// <remarks>
/// The rest of this venue's live user tests trade, so they live in the write block and are approved one at
/// a time. This one asks the question that comes before all of them and costs nothing to ask: does the
/// connector connect at all, against the real exchange, with a real listen key and a real stream.
/// </remarks>
public class UserConnectorReadTests : UserConnectorReadTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorReadTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserConnectorReadTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider, whose connector factory this test resolves.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// The connector connects to the live account and reports its snapshot.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(
        Timeout = TestBlock.ReadTimeoutMs,
        Skip = "needs exchange credentials in test.env",
        SkipUnless = nameof(Exchange.HasCredentials),
        SkipType = typeof(Exchange)
    )]
    public Task ConnectsAndDelivers() =>
        UserConnectorReadBaseAsync(Settings.User, TestContext.Current.CancellationToken);

    /// <summary>
    /// The connection is held open long enough to outlive whatever would quietly end it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This venue keeps its account stream alive differently from the other one: a key fetched over REST
    /// and refreshed on a timer, rather than an answer to a protocol ping. So the thing that can go wrong
    /// is different too - a refresh that stops happening, or one the venue does not accept - and the
    /// symptom is the same either way, a connector that reconnects forever and from outside looks like one
    /// that works.
    /// </para>
    /// <para>
    /// Two and a half minutes does not outlive this venue's key lifetime, which is measured in tens of
    /// minutes; what it outlives is the refresh interval, so a refresh that fails is seen here. The key's
    /// own expiry stays beyond what a suite can wait for, and the reconnect path is what stands in for it.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(
        Timeout = TestBlock.HoldTimeoutMs,
        Skip = "needs exchange credentials in test.env",
        SkipUnless = nameof(Exchange.HasCredentials),
        SkipType = typeof(Exchange)
    )]
    public Task HoldsTheConnection() =>
        UserConnectorHoldBaseAsync(Settings.User, TimeSpan.FromSeconds(150), TestContext.Current.CancellationToken);
}
