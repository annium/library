using Annium.Net.Http;

namespace Annium.Blazor.Net;

public interface IHostHttpRequestFactory
{
    IHttpRequest New();
}
