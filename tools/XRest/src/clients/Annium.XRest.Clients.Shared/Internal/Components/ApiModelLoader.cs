using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Annium.XRest.Clients.Shared.Components;
using Annium.XRest.Core;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.Shared.Internal.Components;

internal class ApiModelLoader : IApiModelLoader
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public ApiModelLoader(IServiceProvider sp)
    {
        _httpRequestFactory = sp.ResolveKeyed<IHttpRequestFactory>(Constants.IndexKey);
    }

    public async Task<ApiModel> LoadAsync(ISourceLoaderConfiguration cfg)
    {
        var model = await _httpRequestFactory.New(cfg.Server).Get(Constants.ApiSourceEndpoint).AsAsync<ApiModel>();

        return model.NotNull();
    }
}
