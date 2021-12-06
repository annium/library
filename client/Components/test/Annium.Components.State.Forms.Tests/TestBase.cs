using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;

namespace Annium.Components.State.Forms.Tests;

public abstract class TestBase
{
    protected IStateFactory GetFactory() => new ServiceContainer()
        .AddRuntimeTools(GetType().Assembly, false)
        .AddMapper()
        .AddComponentFormStateFactory()
        .BuildServiceProvider()
        .Resolve<IStateFactory>();

    protected IValidator<T> GetValidator<T>() => new ServiceContainer()
        .AddRuntimeTools(GetType().Assembly, false)
        .AddValidation()
        .AddLocalization(opts => opts.UseInMemoryStorage())
        .BuildServiceProvider()
        .Resolve<IValidator<T>>();
}