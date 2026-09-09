using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Tables;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User.Requests;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Core.User;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Finance.Providers.Core.Tests.User;

/// <summary>
/// Pins what the user connector factory hands to a provider's own instance factory - the other half of a pair
/// whose market side had the same blind spot: its only callers are the connector test bases, and every test
/// built on those talks to the live exchange and is skipped.
/// </summary>
public class UserConnectorFactoryTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectorFactoryTests"/> class, registering a
    /// stand-in instance factory so the real factory has a provider to build for.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper used to capture test logs.</param>
    public UserConnectorFactoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<CreationLog>().AsSelf().Singleton();
            container.Add<FakeInstanceFactory>().AsKeyed<IUserConnectorInstanceFactory>("fake").Transient();
        });
    }

    /// <summary>
    /// Every connector gets a status monitor of its own, so one connector's components never resolve into
    /// another's aggregate status.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EachConnector_GetsItsOwnMonitor()
    {
        // arrange
        var log = Get<CreationLog>();
        var factory = Get<IUserConnectorFactory>();
        var settings = Settings();

        // act
        await using var first = factory.Create(settings);
        await using var second = factory.Create(settings);

        // assert
        log.Creations.Has(2);
        log.Creations[0].Monitor.IsNotDefault();
        log.Creations[1].Monitor.IsNotDefault();
        ReferenceEquals(log.Creations[0].Monitor, log.Creations[1].Monitor)
            .IsFalse("two connectors sharing one monitor resolve each other's status as their own");
    }

    /// <summary>
    /// The factory builds through whatever provider it was resolved from, rather than opening a scope of its
    /// own.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Factory_BuildsThroughTheProviderItWasResolvedFrom()
    {
        // arrange
        var log = Get<CreationLog>();
        await using var scope = Get<IServiceProvider>().CreateAsyncScope();

        // act
        var factory = scope.ServiceProvider.Resolve<IUserConnectorFactory>();
        await using var connector = factory.Create(Settings());

        // assert
        log.Creations.Has(1);
        log.Creations[0].Sp.Is(scope.ServiceProvider, "the connector was not built through the caller's scope");
    }

    /// <summary>Builds the settings every test here creates a connector for.</summary>
    /// <returns>Settings pointing at the stand-in provider.</returns>
    private static UserSettings Settings() =>
        new()
        {
            Provider = "fake",
            Key = "some_key",
            Secret = "some_secret",
        };

    /// <summary>What an instance factory was handed for one connector.</summary>
    /// <param name="Sp">The provider the instance factory itself was resolved from.</param>
    /// <param name="Monitor">The monitor the connector was told to report into.</param>
    private sealed record Creation(IServiceProvider Sp, IStatusMonitor Monitor);

    /// <summary>Collects what the instance factory saw, in creation order.</summary>
    private sealed class CreationLog
    {
        /// <summary>Gets the creations recorded so far.</summary>
        public List<Creation> Creations { get; } = new();
    }

    /// <summary>
    /// Stands in for a provider's own connector factory, recording what it is handed.
    /// </summary>
    /// <param name="sp">The provider this factory was resolved from.</param>
    /// <param name="log">The log creations are recorded in.</param>
    private sealed class FakeInstanceFactory(IServiceProvider sp, CreationLog log) : IUserConnectorInstanceFactory
    {
        /// <summary>
        /// Records the call and returns a connector that owns nothing.
        /// </summary>
        /// <param name="settings">Ignored.</param>
        /// <param name="monitor">The monitor the caller supplied, recorded.</param>
        /// <param name="disposable">Ignored.</param>
        /// <returns>A connector that does nothing.</returns>
        public IUserConnector Create(UserSettings settings, IStatusMonitor monitor, AsyncDisposableBox disposable)
        {
            log.Creations.Add(new Creation(sp, monitor));

            return new StubConnector();
        }
    }

    /// <summary>A connector that does nothing, so the test observes only the factory's own behaviour.</summary>
    private sealed class StubConnector : IUserConnector
    {
        /// <summary>Gets the connector's status; unused by this test.</summary>
        public ConnectorStatus Status => ConnectorStatus.Connected;

        /// <summary>Gets the asset stream; unused by this test.</summary>
        public IObservable<ChangeEvent<AssetModel>> Assets => Observable.Empty<ChangeEvent<AssetModel>>();

        /// <summary>Gets the position stream; unused by this test.</summary>
        public IObservable<ChangeEvent<PositionModel>> Positions => Observable.Empty<ChangeEvent<PositionModel>>();

        /// <summary>Gets the order stream; unused by this test.</summary>
        public IObservable<ChangeEvent<OrderModel>> Orders => Observable.Empty<ChangeEvent<OrderModel>>();

        /// <summary>Gets the trade stream; unused by this test.</summary>
        public IObservable<TradeModel> Trades => Observable.Empty<TradeModel>();

        /// <summary>Raised on status change; unused by this test.</summary>
        public event Action<ConnectorStatus> OnStatusChanged = delegate { };

        /// <summary>Raised on error; unused by this test.</summary>
        public event Action<ConnectorError> OnError = delegate { };

        /// <summary>Raised on sync; unused by this test.</summary>
        public event Func<UserSettings, IUserProvider, Task> OnSync = delegate
        {
            return Task.CompletedTask;
        };

        /// <summary>Does nothing; unused by this test.</summary>
        public void Sync() => OnStatusChanged(ConnectorStatus.Connected);

        /// <summary>Not used by this test.</summary>
        /// <param name="position">Ignored.</param>
        /// <param name="leverage">Ignored.</param>
        /// <returns>An unsupported result.</returns>
        public Task<UserResult> SetLeverageAsync(PositionModel position, decimal leverage)
        {
            OnError(new ConnectorError("unused"));

            return Task.FromResult(UserResult.New(UserOperationStatus.UnknownError, "unused"));
        }

        /// <summary>Not used by this test.</summary>
        /// <param name="request">Ignored.</param>
        /// <returns>An unsupported result.</returns>
        public Task<UserResult<OrderModel?>> InitOrderAsync(IInitOrderRequest request) =>
            Task.FromResult(UserResult.New<OrderModel?>(UserOperationStatus.UnknownError, null, "unused"));

        /// <summary>Not used by this test.</summary>
        /// <param name="request">Ignored.</param>
        /// <returns>An unsupported result.</returns>
        public Task<UserResult<OrderModel?>> ModifyOrderAsync(IModifyOrderRequest request) =>
            Task.FromResult(UserResult.New<OrderModel?>(UserOperationStatus.UnknownError, null, "unused"));

        /// <summary>Not used by this test.</summary>
        /// <param name="request">Ignored.</param>
        /// <returns>An unsupported result.</returns>
        public Task<UserResult> CancelOrderAsync(ICancelOrderRequest request) =>
            Task.FromResult(UserResult.New(UserOperationStatus.UnknownError, "unused"));

        /// <summary>Not used by this test.</summary>
        /// <param name="symbol">Ignored.</param>
        /// <returns>An unsupported result.</returns>
        public Task<UserResult> CancelAllOrdersAsync(string symbol) =>
            Task.FromResult(UserResult.New(UserOperationStatus.UnknownError, "unused"));

        /// <summary>Does nothing.</summary>
        /// <returns>A completed task.</returns>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
