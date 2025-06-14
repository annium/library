using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Extensions.Validation;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemory;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Base class for all state forms tests providing common setup and utilities
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Initializes a new instance of the TestBase class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddMapper();
            container.AddStateFactory();
            container.AddValidation();
            container.AddLocalization(opts => opts.UseInMemoryStorage());
        });
    }

    /// <summary>
    /// Gets the state factory instance for creating state containers
    /// </summary>
    /// <returns>The state factory instance</returns>
    protected IStateFactory GetFactory() => Get<IStateFactory>();

    /// <summary>
    /// Gets a validator instance for the specified type
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <returns>The validator instance</returns>
    protected IValidator<T> GetValidator<T>() => Get<IValidator<T>>();
}
