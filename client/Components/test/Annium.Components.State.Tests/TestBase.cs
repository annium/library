using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Components.State.Tests
{
    public abstract class TestBase
    {
        protected IStateFactory GetFactory() => new ServiceCollection()
            .AddRuntimeTools(GetType().Assembly)
            .AddMapper()
            .AddComponentStateFactory()
            .BuildServiceProvider()
            .GetRequiredService<IStateFactory>();
    }
}