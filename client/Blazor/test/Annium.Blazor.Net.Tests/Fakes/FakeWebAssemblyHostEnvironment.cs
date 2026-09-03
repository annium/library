using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Net.Tests.Fakes;

/// <summary>
/// A fake <see cref="IWebAssemblyHostEnvironment"/> exposing a fixed <see cref="BaseAddress"/>, so
/// <c>HostHttpRequestFactory</c>'s derivation of its base URI from the host can be asserted deterministically.
/// </summary>
internal sealed class FakeWebAssemblyHostEnvironment : IWebAssemblyHostEnvironment
{
    /// <summary>
    /// The fixed base address surfaced by this fake; tests assert the request factory is wired with it.
    /// </summary>
    public const string TestBaseAddress = "https://example.com/";

    /// <summary>
    /// Gets the base address for the application (the fixed <see cref="TestBaseAddress"/>).
    /// </summary>
    public string BaseAddress => TestBaseAddress;

    /// <summary>
    /// Gets the environment name (fixed to "Production").
    /// </summary>
    public string Environment => "Production";
}
