using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Components.Forms.Tests
{
    public abstract class TestBase
    {
        protected IStateFactory GetFactory() => new ServiceCollection()
            .AddRuntimeTools(GetType().Assembly)
            .AddMapper()
            .AddFormsState()
            .BuildServiceProvider()
            .GetRequiredService<IStateFactory>();
    }
}