using System.Threading.Tasks;
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
}
