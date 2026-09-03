using System.Reflection;
using Annium.Blazor.Interop;
using Annium.Blazor.Interop.Tests.Fakes;
using Xunit.Sdk;
using Xunit.v3;

// InteropContext.Instance is a process-wide static singleton; these tests mutate its recorded state, so they must
// not run in parallel with one another.
[assembly: Parallelization(Mode = ParallelMode.None)]

namespace Annium.Blazor.Interop.Tests;

/// <summary>
/// Base for interop tests: installs a shared <see cref="FakeInteropContext"/> as the process-wide
/// <see cref="InteropContext"/> singleton (once, via reflection over its private setter) and resets it before each
/// test so recorded JS calls never leak between tests.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// The shared fake context installed as <see cref="InteropContext.Instance"/> for the whole test run.
    /// </summary>
    private static readonly FakeInteropContext _fake = InstallFake();

    /// <summary>
    /// Gets the fake context backing <see cref="InteropContext.Instance"/> for the current test.
    /// </summary>
    private protected FakeInteropContext Fake => _fake;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class, resetting the shared fake so each test starts
    /// with a clean recording.
    /// </summary>
    protected TestBase()
    {
        _fake.Reset();
    }

    /// <summary>
    /// Creates the fake context and assigns it to the private <see cref="InteropContext.Instance"/> setter.
    /// </summary>
    /// <returns>The installed fake context.</returns>
    private static FakeInteropContext InstallFake()
    {
        var fake = new FakeInteropContext();
        typeof(InteropContext)
            .GetProperty(nameof(InteropContext.Instance), BindingFlags.Public | BindingFlags.Static)!
            .SetValue(null, fake);

        return fake;
    }
}
