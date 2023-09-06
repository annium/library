using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;
using Xunit.Abstractions;

namespace Annium.Components.State.Forms.Tests;

public abstract class TestBase : Testing.Lib.TestBase
{
    protected TestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(container =>
        {
            container.AddMapper();
            container.AddStateFactory();
            container.AddValidation();
            container.AddLocalization(opts => opts.UseInMemoryStorage());
        });
    }

    protected IStateFactory GetFactory() => Get<IStateFactory>();

    protected IValidator<T> GetValidator<T>() => Get<IValidator<T>>();
}