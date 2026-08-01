using System.ComponentModel;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.Shared;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Xunit;

namespace Annium.Integrations.AI.SemanticKernel.Tests;

/// <summary>
/// Tests for how plugin sources are collected into the resolved <see cref="Kernel"/>.
/// </summary>
public class PluginRegistrationTests
{
    /// <summary>
    /// A plugin implementing <see cref="ISemanticKernelPlugin"/> is discovered and exposed on the kernel
    /// together with its functions.
    /// </summary>
    [Fact]
    public void WithPluginInstances_ExposesPluginOnKernel()
    {
        // arrange
        var container = Container();
        container.AddSemanticKernel().WithPluginInstances();
        var provider = container.BuildServiceProvider();

        // act
        var kernel = provider.Resolve<Kernel>();

        // assert
        kernel.Plugins.Has(1);
        var plugin = kernel.Plugins.Single();
        plugin.Select(x => x.Name).Has(1).At(0).Is(nameof(EchoPlugin.Echo));
    }

    /// <summary>
    /// A kernel built without any plugin source resolves with an empty plugin collection instead of failing.
    /// </summary>
    [Fact]
    public void AddSemanticKernel_NoPluginSources_ResolvesEmptyKernel()
    {
        // arrange
        var container = Container();
        container.AddSemanticKernel();
        var provider = container.BuildServiceProvider();

        // act
        var kernel = provider.Resolve<Kernel>();

        // assert
        kernel.Plugins.IsEmpty();
    }

    /// <summary>
    /// Plugin collections registered by several sources are merged rather than shadowing one another.
    /// </summary>
    [Fact]
    public void AddSemanticKernel_SeveralPluginCollections_AreMerged()
    {
        // arrange
        var container = Container();
        container.AddSemanticKernel().WithPluginInstances();
        container
            .Add(_ => new KernelPluginCollection([KernelPluginFactory.CreateFromObject(new TimePlugin(), "time")]))
            .AsSelf()
            .Singleton();
        var provider = container.BuildServiceProvider();

        // act
        var kernel = provider.Resolve<Kernel>();

        // assert
        kernel.Plugins.Has(2);
        kernel.Plugins.Any(x => x.Name == "time").IsTrue();
    }

    /// <summary>
    /// Builds a container with the runtime scanning and logging the kernel registrations depend on.
    /// </summary>
    /// <returns>A container ready for Semantic Kernel registrations.</returns>
    private static IServiceContainer Container()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(PluginRegistrationTests).Assembly);
        container.AddLogging();
        container.Collection.AddLogging();

        return container;
    }
}

/// <summary>
/// Plugin used to verify discovery through <see cref="ISemanticKernelPlugin"/>.
/// </summary>
public class EchoPlugin : ISemanticKernelPlugin
{
    /// <summary>
    /// Returns the given text unchanged.
    /// </summary>
    /// <param name="text">The text to echo.</param>
    /// <returns>The text, unchanged.</returns>
    [KernelFunction]
    [Description("Echoes the given text")]
    public string Echo(string text) => text;
}

/// <summary>
/// Plugin registered directly as a collection, without going through discovery.
/// </summary>
public class TimePlugin
{
    /// <summary>
    /// Returns a fixed timestamp.
    /// </summary>
    /// <returns>A fixed timestamp.</returns>
    [KernelFunction]
    [Description("Returns a fixed timestamp")]
    public string Now() => "2026-08-01T00:00:00Z";
}
