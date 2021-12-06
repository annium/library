using Annium.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Annium.Blazor.Net.Internal;

internal class HostHttpRequestFactory : IHostHttpRequestFactory
{
    private readonly IHttpRequest _request;

    public HostHttpRequestFactory(
        IHttpRequestFactory requestFactory,
        IWebAssemblyHostEnvironment hostEnvironment
    )
    {
        _request = requestFactory.New(hostEnvironment.BaseAddress);
    }

    public IHttpRequest New() => _request.Clone();
}