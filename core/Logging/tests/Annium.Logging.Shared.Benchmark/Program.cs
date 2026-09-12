using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Perfolizer.Horology;

var config = new ManualConfig()
    .AddExporter(MarkdownExporter.Default)
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddJob(
        Job.Default.WithWarmupCount(1)
            .WithLaunchCount(3)
            .WithIterationCount(5)
            // a message queued for the background route stays queued until the Rx pump drains it, and the
            // producer is faster than the pump. A default-length iteration would leave tens of millions of
            // messages in flight; a short one keeps the working set bounded. Allocations per operation are
            // exact either way — it is the timing precision that is traded, and ballpark timing is what
            // this run is for.
            .WithIterationTime(TimeInterval.FromMilliseconds(100))
            .WithStrategy(RunStrategy.Throughput)
            .WithPlatform(Platform.AnyCpu)
            .WithRuntime(CoreRuntime.Latest)
    )
    .AddValidator(JitOptimizationsValidator.DontFailOnError)
    .AddLogger(ConsoleLogger.Default)
    .AddColumnProvider(DefaultColumnProviders.Instance);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
