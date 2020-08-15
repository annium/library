using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;
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

        protected IValidator<T> GetValidator<T>() => new ServiceCollection()
            .AddRuntimeTools(GetType().Assembly)
            .AddValidation()
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .GetRequiredService<IValidator<T>>();
    }
}