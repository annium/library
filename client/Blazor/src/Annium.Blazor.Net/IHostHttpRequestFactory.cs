using Annium.Net.Http;

namespace Annium.Blazor.Net;

/// <summary>
/// Factory interface for creating HTTP requests configured with the host's base address.
/// </summary>
public interface IHostHttpRequestFactory
{
    /// <summary>
    /// Creates a new HTTP request configured with the host's base address.
    /// </summary>
    /// <returns>A new HTTP request instance.</returns>
    IHttpRequest New();
}
