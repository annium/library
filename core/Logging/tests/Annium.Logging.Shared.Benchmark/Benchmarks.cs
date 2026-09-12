using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared.Benchmark.Internal;
using BenchmarkDotNet.Attributes;

namespace Annium.Logging.Shared.Benchmark;

/// <summary>
/// Measures what one log message costs on the calling thread while the level is on.
/// </summary>
/// <remarks>
/// <para>
/// Exists to answer a question the logging-cost research left open: whether caching the template parse,
/// building the structured-data dictionary lazily and caching the source-file name are worth the work, or
/// whether batching the console output already settled it. The backtest re-measurement that would answer
/// it end to end lives in another repository and waits on another change; this measures the same path
/// here, per message, with allocation counts.
/// </para>
/// <para>
/// The three <c>Produce_*</c> and <c>Dispatch_*</c> groups go through <see cref="ILogSentryBridge"/>
/// directly, below the level gate, because the gate is the one part already known to be free. The
/// <c>Gate_*</c> pair goes through the call-site API instead, which is where the gate sits.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Benchmarks
{
    /// <summary>
    /// How many distinct pre-rendered strings <see cref="Produce_PreRendered"/> cycles through. A power
    /// of two so the cursor wraps with a mask.
    /// </summary>
    private const int RenderedCount = 64;

    /// <summary>
    /// A source-file path of the shape <c>[CallerFilePath]</c> supplies — a compile-time constant, the
    /// same instance on every call from a given site.
    /// </summary>
    private const string File = "/Users/dev/project/src/Some/Namespace/SomeService.cs";

    /// <summary>Member name of the shape <c>[CallerMemberName]</c> supplies.</summary>
    private const string Member = "HandleAsync";

    /// <summary>A template with no placeholders.</summary>
    private const string Template0 = "connection established";

    /// <summary>A template with one placeholder.</summary>
    private const string Template1 = "order {id} accepted";

    /// <summary>A template with two placeholders.</summary>
    private const string Template2 = "order {id} filled at {price}";

    /// <summary>
    /// Bridge of the route that rejects everything — the producer on its own.
    /// </summary>
    private readonly ILogSentryBridge _rejecting;

    /// <summary>
    /// Bridge of the route that accepts everything through the immediate scheduler.
    /// </summary>
    private readonly ILogSentryBridge _immediate;

    /// <summary>
    /// Bridge of the route whose sink reads each message's structured data.
    /// </summary>
    private readonly ILogSentryBridge _readingData;

    /// <summary>
    /// Bridge of the route that accepts everything through the background scheduler.
    /// </summary>
    private readonly ILogSentryBridge _background;

    /// <summary>
    /// Subject for the call-site seam, obtained as a log bridge so the subject type is a stored string
    /// rather than a reflected type name.
    /// </summary>
    private readonly ILogSubject _subject;

    /// <summary>
    /// Providers owning the three routes, kept so they can be torn down at the end of the run.
    /// </summary>
    private readonly IServiceProviderContainer[] _providers;

    /// <summary>
    /// Distinct, already-rendered strings standing in for what the Microsoft.Extensions.Logging bridge
    /// passes as a template: the output of its formatter, a fresh string on every record.
    /// </summary>
    private readonly string[] _rendered;

    /// <summary>
    /// Cursor into <see cref="_rendered"/>.
    /// </summary>
    private int _cursor;

    /// <summary>
    /// Argument for the <c>Gate_*</c> pair. A value type rather than a string because the call-site
    /// overloads are ambiguous for a string second argument — it matches both the no-placeholder overload's
    /// <c>[CallerFilePath]</c> parameter and the one-placeholder overload's value.
    /// </summary>
    private readonly long _id = 42L;

    /// <summary>
    /// Builds the three providers, resolves their bridges, and pins the global level to
    /// <see cref="LogLevel.Info"/> so the <c>Gate_*</c> pair has one gated level and one open one.
    /// </summary>
    public Benchmarks()
    {
        var rejecting = Routes.Rejecting();
        var immediate = Routes.Immediate();
        var readingData = Routes.ReadingData();
        var background = Routes.Background();
        _providers = [rejecting, immediate, readingData, background];

        _rejecting = rejecting.Resolve<ILogSentryBridge>();
        _immediate = immediate.Resolve<ILogSentryBridge>();
        _readingData = readingData.Resolve<ILogSentryBridge>();
        _background = background.Resolve<ILogSentryBridge>();
        _subject = immediate.Resolve<ILogBridgeFactory>().Get("benchmark");

        _rendered = new string[RenderedCount];
        for (var i = 0; i < RenderedCount; i++)
            _rendered[i] = $"request {i} completed in {i * 7} ms";

        // Trace is then gated and Info is not, which is the shape a host runs in.
        LogConfig.SetLevel(LogLevel.Info);
    }

    /// <summary>
    /// Tears the providers down, which drains the background route rather than dropping what it queued.
    /// </summary>
    /// <returns>A task that completes once every provider is disposed.</returns>
    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        foreach (var provider in _providers)
            await provider.DisposeAsync();
    }

    /// <summary>
    /// Producer cost for a template with no placeholders.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void Produce_NoPlaceholders() =>
        _rejecting.Register("bench", "1", File, Member, 42, LogLevel.Info, Template0, null, []);

    /// <summary>
    /// Producer cost for one placeholder bound to a reference type.
    /// </summary>
    [Benchmark]
    public void Produce_OnePlaceholder() =>
        _rejecting.Register("bench", "1", File, Member, 42, LogLevel.Info, Template1, null, ["abc123"]);

    /// <summary>
    /// Producer cost for one placeholder bound to a value type, which the <c>object?</c> argument list
    /// boxes on the way in.
    /// </summary>
    [Benchmark]
    public void Produce_OneValueTypePlaceholder() =>
        _rejecting.Register("bench", "1", File, Member, 42, LogLevel.Info, Template1, null, [42L]);

    /// <summary>
    /// Producer cost for two placeholders — the typical call.
    /// </summary>
    [Benchmark]
    public void Produce_TwoPlaceholders() =>
        _rejecting.Register("bench", "1", File, Member, 42, LogLevel.Info, Template2, null, ["abc123", 1234.5m]);

    /// <summary>
    /// Producer cost on the Microsoft.Extensions.Logging bridge's path: an already-rendered string as the
    /// template, a fresh instance every time, and no data items at all.
    /// </summary>
    /// <remarks>
    /// The strings are pre-generated rather than built here, so their allocation is not charged to the
    /// measurement — what is measured is the parse of a string the producer has never seen before. Note
    /// that the array keeps all <see cref="RenderedCount"/> of them alive, so a per-template cache would
    /// hit after one lap; the honest reading of this benchmark is the cost of the parse itself, and the
    /// reason a cache on this path has to be bounded by something other than the number of call sites.
    /// </remarks>
    [Benchmark]
    public void Produce_PreRendered()
    {
        var rendered = _rendered[_cursor = (_cursor + 1) & (RenderedCount - 1)];
        _rejecting.Register("bench", "1", string.Empty, string.Empty, 0, LogLevel.Info, rendered, null, []);
    }

    /// <summary>
    /// The same message as <see cref="Produce_OnePlaceholder"/> accepted and dispatched synchronously.
    /// The difference between the two is what the immediate scheduler adds.
    /// </summary>
    [Benchmark]
    public void Dispatch_Immediate() =>
        _immediate.Register("bench", "1", File, Member, 42, LogLevel.Info, Template1, null, ["abc123"]);

    /// <summary>
    /// The same message again, dispatched to a sink that reads its structured data the way the Graylog
    /// and Seq sinks do.
    /// </summary>
    /// <remarks>
    /// While the dictionary is built for every message, the gap from <see cref="Dispatch_Immediate"/> is
    /// only the enumerator the read itself boxes — the build is already paid on both sides. Once the
    /// dictionary is built lazily, the pair separates the other way: this one keeps paying for it because
    /// it asks, and <c>Dispatch_Immediate</c> stops. So the signal is not the gap widening but
    /// <c>Dispatch_Immediate</c> falling while this one holds.
    /// </remarks>
    [Benchmark]
    public void Dispatch_ReadingData() =>
        _readingData.Register("bench", "1", File, Member, 42, LogLevel.Info, Template1, null, ["abc123"]);

    /// <summary>
    /// The same message queued for a background pump. The difference from
    /// <see cref="Produce_OnePlaceholder"/> is the per-message lock and channel write.
    /// </summary>
    [Benchmark]
    public void Dispatch_Background() =>
        _background.Register("bench", "1", File, Member, 42, LogLevel.Info, Template1, null, ["abc123"]);

    /// <summary>
    /// A call at a gated level, through the call-site API. Nothing should be produced, and the arguments
    /// are locals, so this is the cost of the gate and the argument list alone.
    /// </summary>
    [Benchmark]
    public void Gate_Disabled() => _subject.Trace(Template1, _id);

    /// <summary>
    /// The same call at an open level, for what the gate is saving.
    /// </summary>
    [Benchmark]
    public void Gate_Enabled() => _subject.Info(Template1, _id);

    /// <summary>
    /// The level probe external bridges use to skip building a message at all.
    /// </summary>
    /// <returns>Whether the route accepts <see cref="LogLevel.Info"/>.</returns>
    [Benchmark]
    public bool ProbeLevel() => _immediate.IsLevelEnabled(LogLevel.Info);
}
