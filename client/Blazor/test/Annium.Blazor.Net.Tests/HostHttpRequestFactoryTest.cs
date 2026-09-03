using System;
using Annium.Blazor.Net.Tests.Fakes;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Annium.Testing;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Xunit;

namespace Annium.Blazor.Net.Tests;

/// <summary>
/// Tests for the host HTTP request factory — the only executable logic in <c>Annium.Blazor.Net</c>: it must derive
/// its base URI from the WebAssembly host environment and apply that base to every request it produces. Exercised
/// through the real <see cref="ServiceContainerExtensions.AddHostHttpRequestFactory"/> registration (the internal
/// implementation is resolved by its public interface), with the HTTP stack and host environment faked.
/// </summary>
public class HostHttpRequestFactoryTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostHttpRequestFactoryTest"/> class, wiring the factory under
    /// test against a recording HTTP-request factory and a fixed-base-address host environment.
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging.</param>
    public HostHttpRequestFactoryTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddHostHttpRequestFactory();
            container.Add<FakeHttpRequestFactory>().AsSelf().As<IHttpRequestFactory>().Singleton();
            container.Add<FakeWebAssemblyHostEnvironment>().As<IWebAssemblyHostEnvironment>().Singleton();
        });
    }

    /// <summary>
    /// Tests that a request produced by the factory carries the host's base address: the factory must build its base
    /// <see cref="Uri"/> from <see cref="IWebAssemblyHostEnvironment.BaseAddress"/> and pass it to the underlying
    /// request factory. Pins the host-base-address contract against a dropped/wrong base or swapped constructor args.
    /// </summary>
    [Fact]
    public void New_AppliesHostBaseAddress()
    {
        var factory = Get<IHostHttpRequestFactory>();

        factory.New();

        Get<FakeHttpRequestFactory>().LastBaseUri.Is(new Uri(FakeWebAssemblyHostEnvironment.TestBaseAddress));
    }
}
