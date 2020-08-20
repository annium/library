using System;
using Annium.Components.State.Operations;

namespace Annium.Components.Operations.Tests
{
    public abstract class TestBase
    {
        protected (T, Func<int>) Arrange<T>(Func<T> factory)
            where T : IOperationStateBase
        {
            var op = factory();
            var changesCounter = 0;
            op.Changed.Subscribe(_ => ++changesCounter);

            return (op, () => changesCounter);
        }
    }
}