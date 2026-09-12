using BenchmarkDotNet.Attributes;

namespace Annium.Data.Operations.Benchmark;

/// <summary>
/// Measures what a result costs, separating the successful path from the one that carries errors.
/// </summary>
/// <remarks>
/// The allocation profile of a backtest put the error collections of <c>ResultBase</c> at roughly 6% of
/// all allocations, which is a share paid by every application built on Annium and not only by the one
/// that was profiled. What the profile could not say is how much of it the successful path pays, and
/// that is the split these rows draw.
/// </remarks>
[MemoryDiagnoser]
public class Benchmarks
{
    /// <summary>
    /// A result carrying no data and no errors — the shape a successful void operation returns.
    /// </summary>
    /// <returns>The result, so the call is not elided.</returns>
    [Benchmark(Baseline = true)]
    public IResult Create() => Result.Create();

    /// <summary>
    /// A result carrying data and no errors.
    /// </summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public IResult<int> CreateWithData() => Result.Create(5);

    /// <summary>
    /// Creating a successful result and asking whether it is ok, which is what a caller does with one.
    /// </summary>
    /// <returns>Whether the result is ok.</returns>
    [Benchmark]
    public bool CreateAndCheck() => Result.Create(5).IsOk;

    /// <summary>
    /// Creating a result and reading its empty error collections — the accessors a failed-path branch
    /// reaches for even when the result turns out fine.
    /// </summary>
    /// <returns>The number of plain errors, which is zero.</returns>
    [Benchmark]
    public int CreateAndReadErrors()
    {
        var result = Result.Create();

        return result.PlainErrors.Count + result.LabeledErrors.Count;
    }

    /// <summary>
    /// Copying a successful result, which passes its empty error collections through <c>Errors</c>.
    /// </summary>
    /// <returns>The copy.</returns>
    [Benchmark]
    public IResult<int> CopySuccessful() => Result.Create(5).Copy();

    /// <summary>
    /// A result carrying one plain error — the path that does need the collections.
    /// </summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public IResult CreateWithError() => Result.Create().Error("failed");

    /// <summary>
    /// A result carrying one labeled error.
    /// </summary>
    /// <returns>The result.</returns>
    [Benchmark]
    public IResult CreateWithLabeledError() => Result.Create().Error("field", "failed");
}
