using System;
using Annium.Components.State.Operations;
using Xunit;

namespace Annium.Components.Operations.Tests;

/// <summary>
/// Base test class providing common test utilities for operation state testing.
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Initializes a new instance of the TestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Arranges an operation state instance with change tracking for testing.
    /// </summary>
    /// <typeparam name="T">The type of operation state to create.</typeparam>
    /// <param name="factory">Factory function to create the operation state instance.</param>
    /// <returns>A tuple containing the operation state instance and a function to get the change count.</returns>
    protected (T, Func<int>) Arrange<T>(Func<T> factory)
        where T : IOperationStateBase
    {
        var op = factory();
        var changesCounter = 0;
        op.Changed.Subscribe(_ => ++changesCounter);

        return (op, () => changesCounter);
    }
}
