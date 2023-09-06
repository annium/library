using System;
using Annium.Components.State.Operations;
using Xunit.Abstractions;

namespace Annium.Components.Operations.Tests;

public abstract class TestBase : Testing.Lib.TestBase
{
    protected TestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected (T, Func<int>) Arrange<T>(Func<T> factory)
        where T : IOperationStateBase
    {
        var op = factory();
        var changesCounter = 0;
        op.Changed.Subscribe(_ => ++changesCounter);

        return (op, () => changesCounter);
    }
}