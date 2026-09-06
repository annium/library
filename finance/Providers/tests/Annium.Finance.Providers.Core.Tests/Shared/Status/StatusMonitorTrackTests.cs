using System.Collections.Concurrent;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using static Annium.Finance.Providers.Abstractions.Connectors.Shared.ConnectorStatus;

namespace Annium.Finance.Providers.Core.Tests.Shared.Status;

/// <summary>
/// Pins how a component that reports through a monitor of its own is counted by another monitor. This is what
/// a connector does with the provider's server time source: the source is shared by every connector on the
/// provider, so it cannot report into any one of theirs, and each connector mirrors it in instead - which is
/// what keeps "not connected until the clock is synced" true of a connector that no longer owns a time source.
/// </summary>
public class StatusMonitorTrackTests : ProvidersTestBase
{
    /// <summary>The monitor standing in for a connector's aggregate.</summary>
    private StatusMonitor Connector => field ??= new StatusMonitor(Get<ILogger>());

    /// <summary>The monitor standing in for the shared component's own.</summary>
    private StatusMonitor Shared => field ??= new StatusMonitor(Get<ILogger>());

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusMonitorTrackTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper used to capture test logs.</param>
    public StatusMonitorTrackTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A tracked component counts towards the tracking monitor's aggregate: connected only once it is, and
    /// dropping the aggregate back to connecting when it drops.
    /// </summary>
    [Fact]
    public void TrackedComponent_CountsTowardsTheAggregate()
    {
        // arrange - the connector's own target is connected, so the tracked one decides the aggregate
        var own = Connector.CreateReporter();
        own.Bind(new Own(), Connected);

        var component = new SharedComponent();
        var componentReporter = Shared.CreateReporter();
        componentReporter.Bind(component, Connecting);

        // act
        using var tracking = Connector.Track(Shared, component);

        // assert - the tracked component is not connected, so neither is the aggregate
        Connector.Status.Is(Connecting, "a component that is still connecting must hold the aggregate back");

        // act
        componentReporter.Connected();

        // assert
        Connector.Status.Is(Connected, "the aggregate must follow the tracked component up");

        // act
        componentReporter.Disconnected();

        // assert
        Connector.Status.Is(Connecting, "the aggregate must follow the tracked component down");
    }

    /// <summary>
    /// Disposing the handle stops counting the component. A connector that has gone away must not leave a
    /// target behind in a monitor that outlives it - and the shared component outlives every connector.
    /// </summary>
    [Fact]
    public void DisposedTracking_StopsCountingTheComponent()
    {
        // arrange
        var own = Connector.CreateReporter();
        own.Bind(new Own(), Connected);

        var component = new SharedComponent();
        var componentReporter = Shared.CreateReporter();
        componentReporter.Bind(component, Connecting);

        var tracking = Connector.Track(Shared, component);
        Connector.Status.Is(Connecting);

        // act
        tracking.Dispose();

        // assert - only the connector's own target is left, and it is connected
        Connector.Status.Is(Connected, "an untracked component must no longer hold the aggregate back");

        // act - and the component going on with its life reaches nobody
        componentReporter.Disconnected();

        // assert
        Connector.Status.Is(Connected, "a released tracking must not keep following the component");
    }

    /// <summary>
    /// An error the shared component reports reaches the tracking monitor's listeners. The monitor is the
    /// only path an error takes to a connector's consumer, so a shared component's failures would otherwise
    /// go unmentioned.
    /// </summary>
    [Fact]
    public void ErrorFromTheComponent_ReachesTheTrackingMonitor()
    {
        // arrange
        var errors = new ConcurrentQueue<ConnectorError>();
        Connector.OnError += errors.Enqueue;

        var component = new SharedComponent();
        var componentReporter = Shared.CreateReporter();
        componentReporter.Bind(component);

        using var tracking = Connector.Track(Shared, component);

        // act
        var error = new ConnectorError("the shared component failed");
        componentReporter.Error(error);

        // assert
        errors.IsEqual(new[] { error });
    }

    /// <summary>Stands in for the connector's own target.</summary>
    private sealed class Own;

    /// <summary>Stands in for a component shared by several connectors.</summary>
    private sealed class SharedComponent;
}
