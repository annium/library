using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;

namespace Annium.Components.State.Forms.Tests;

public abstract class TestBase
{
    protected IStateFactory GetFactory() => new ServiceContainer()
        .AddRuntime(GetType().Assembly)
        .AddMapper()
        .AddStateFactory()
        .BuildServiceProvider()
        .Resolve<IStateFactory>();

    protected IValidator<T> GetValidator<T>() => new ServiceContainer()
        .AddRuntime(GetType().Assembly)
        .AddValidation()
        .AddLocalization(opts => opts.UseInMemoryStorage())
        .BuildServiceProvider()
        .Resolve<IValidator<T>>();
}