using System;
using Annium.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Net.Internal;

/// <summary>
/// Internal implementation of the host HTTP request factory that creates HTTP requests configured with the WebAssembly host's base address.
/// </summary>
internal class HostHttpRequestFactory : IHostHttpRequestFactory
{
    /// <summary>
    /// The underlying HTTP request factory.
    /// </summary>
    private readonly IHttpRequestFactory _requestFactory;

    /// <summary>
    /// The base address of the WebAssembly host.
    /// </summary>
    private readonly Uri _baseAddress;

    /// <summary>
    /// Initializes a new instance of the HostHttpRequestFactory class.
    /// </summary>
    /// <param name="requestFactory">The HTTP request factory to use for creating requests.</param>
    /// <param name="hostEnvironment">The WebAssembly host environment containing the base address.</param>
    public HostHttpRequestFactory(IHttpRequestFactory requestFactory, IWebAssemblyHostEnvironment hostEnvironment)
    {
        _requestFactory = requestFactory;
        _baseAddress = new Uri(hostEnvironment.BaseAddress);
    }

    /// <summary>
    /// Creates a new HTTP request configured with the host's base address.
    /// </summary>
    /// <returns>A new HTTP request instance configured with the host's base address.</returns>
    public IHttpRequest New() => _requestFactory.New(_baseAddress);
}
