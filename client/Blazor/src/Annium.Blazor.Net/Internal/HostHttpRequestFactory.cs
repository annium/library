using System;
using Annium.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Net.Internal;

internal class HostHttpRequestFactory : IHostHttpRequestFactory
{
    private readonly IHttpRequestFactory _requestFactory;
    private readonly Uri _baseAddress;

    public HostHttpRequestFactory(
        IHttpRequestFactory requestFactory,
        IWebAssemblyHostEnvironment hostEnvironment
    )
    {
        _requestFactory = requestFactory;
        _baseAddress = new Uri(hostEnvironment.BaseAddress);
    }

    public IHttpRequest New() => _requestFactory.New(_baseAddress);
}