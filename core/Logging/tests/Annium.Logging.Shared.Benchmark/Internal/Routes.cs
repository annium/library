using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;

namespace Annium.Logging.Shared.Benchmark.Internal;

/// <summary>
/// Builds the three providers the benchmarks measure against. Each differs only in the single route it
/// registers, so the difference between two of their numbers is the cost of what the route adds.
/// </summary>
internal static class Routes
{
    /// <summary>
    /// A provider whose only route rejects every message. Isolates the producer: the caller pays for
    /// <c>Helper.Process</c>, the <see cref="LogMessage{TContext}"/> and the source-file name, then the
    /// router's filter says no and nothing else happens.
    /// </summary>
    /// <returns>The built provider.</returns>
    public static IServiceProviderContainer Rejecting() => Build(route => route.For(_ => false).Use(new NoOpSink()));

    /// <summary>
    /// A provider whose only route accepts everything and dispatches through the immediate scheduler.
    /// Adds, over <see cref="Rejecting"/>, the one-element batch array and the synchronous hand-off.
    /// </summary>
    /// <returns>The built provider.</returns>
    public static IServiceProviderContainer Immediate() => Build(route => route.ForAll().Use(new NoOpSink()));

    /// <summary>
    /// A provider whose only route accepts everything and reads each message's structured data, the way
    /// the Graylog and Seq sinks do. Adds, over <see cref="Immediate"/>, only that read — so the gap
    /// between the two is what the structured-data dictionary costs.
    /// </summary>
    /// <returns>The built provider.</returns>
    public static IServiceProviderContainer ReadingData() => Build(route => route.ForAll().Use(new DataReadingSink()));

    /// <summary>
    /// A provider whose only route accepts everything and queues through the background scheduler.
    /// Adds, over <see cref="Rejecting"/>, the per-message lock and channel write — the baseline for
    /// the question of whether that lock is worth removing.
    /// </summary>
    /// <returns>The built provider.</returns>
    public static IServiceProviderContainer Background() =>
        Build(route => route.ForAll().Use(new NoOpSink()).WithBackgroundScheduler());

    /// <summary>
    /// Assembles a container with real time and default-context logging, builds it, and applies the
    /// given single route.
    /// </summary>
    /// <param name="configure">Configures the one route the provider carries.</param>
    /// <returns>The built provider.</returns>
    private static IServiceProviderContainer Build(Action<LogRoute<DefaultLogContext>> configure)
    {
        var container = new ServiceContainer();
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();

        var provider = container.BuildServiceProvider();
        provider.UseLogging(configure);

        return provider;
    }
}
