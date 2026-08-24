using System;
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
        // the plugin is named after its own type, which is how a caller addresses it from a prompt
        plugin.Name.Is(nameof(EchoPlugin));
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
    /// The kernel is transient: every resolution hands out its own instance, so a caller mutating one
    /// kernel's plugins or data does not reach into anybody else's.
    /// </summary>
    [Fact]
    public void AddSemanticKernel_ResolvedTwice_ReturnsDistinctKernels()
    {
        // arrange
        var container = Container();
        container.AddSemanticKernel().WithPluginInstances();
        var provider = container.BuildServiceProvider();

        // act
        var first = provider.Resolve<Kernel>();
        var second = provider.Resolve<Kernel>();

        // assert
        ReferenceEquals(first, second).IsFalse("the kernel registration is transient");
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
    /// Plugin names must be unique across every source: two collections offering the same name make the
    /// kernel unresolvable, so the collision surfaces on every resolution rather than silently dropping one.
    /// </summary>
    [Fact]
    public void AddSemanticKernel_DuplicatePluginName_FailsToResolveKernel()
    {
        // arrange - the discovered EchoPlugin is named after its type; a second source claims that name
        var container = Container();
        container.AddSemanticKernel().WithPluginInstances();
        container
            .Add(_ => new KernelPluginCollection([
                KernelPluginFactory.CreateFromObject(new TimePlugin(), nameof(EchoPlugin)),
            ]))
            .AsSelf()
            .Singleton();
        var provider = container.BuildServiceProvider();

        // act & assert - Semantic Kernel's own collection rejects the duplicate; the merge does not
        // de-duplicate, so this is a caller error that shows up at resolution
        Wrap.It(() => provider.Resolve<Kernel>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Discovery without <c>AddRuntime</c> fails loudly at the registration call, rather than leaving a
    /// kernel that quietly has no plugins.
    /// </summary>
    [Fact]
    public void WithPluginInstances_WithoutRuntime_ThrowsAtRegistration()
    {
        // arrange - deliberately no AddRuntime, so no type manager exists to ask for implementations
        var container = new ServiceContainer();
        container.AddLogging();
        container.Collection.AddLogging();
        var builder = container.AddSemanticKernel();

        // act & assert - the misconfiguration surfaces here, not at kernel resolution
        Wrap.It(() => builder.WithPluginInstances()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Scanning an assembly that declares no plugins is the silent case: the kernel resolves with an
    /// empty plugin collection instead of failing.
    /// </summary>
    [Fact]
    public void WithPluginInstances_ScannedAssemblyWithoutPlugins_ResolvesEmptyKernel()
    {
        // arrange - a real assembly is scanned, it simply holds no ISemanticKernelPlugin implementation
        var container = new ServiceContainer();
        container.AddRuntime(typeof(Kernel).Assembly);
        container.AddLogging();
        container.Collection.AddLogging();
        container.AddSemanticKernel().WithPluginInstances();
        var provider = container.BuildServiceProvider();

        // act
        var kernel = provider.Resolve<Kernel>();

        // assert
        kernel.Plugins.IsEmpty();
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
