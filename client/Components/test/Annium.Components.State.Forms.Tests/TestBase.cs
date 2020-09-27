using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Components.State.Forms.Tests
{
    public abstract class TestBase
    {
        protected IStateFactory GetFactory() => new ServiceCollection()
            .AddRuntimeTools(GetType().Assembly, false)
            .AddMapper()
            .AddComponentFormStateFactory()
            .BuildServiceProvider()
            .GetRequiredService<IStateFactory>();

        protected IValidator<T> GetValidator<T>() => new ServiceCollection()
            .AddRuntimeTools(GetType().Assembly, false)
            .AddValidation()
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .GetRequiredService<IValidator<T>>();
    }
}