using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Components.State.Forms.Tests
{
    public abstract class TestBase
    {
        protected IStateFactory GetFactory() => new ServiceCollection()
            .AddRuntimeTools(GetType().Assembly)
            .AddMapper()
            .AddComponentFormStateFactory()
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