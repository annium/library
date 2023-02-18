using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using XRest.Clients.Shared.Components;
using XRest.Core;
using XRest.Core.Models;

namespace XRest.Clients.Shared.Internal.Components;

internal class Loader : ILoader
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public Loader(
        IIndex<string, IHttpRequestFactory> httpRequestFactories
    )
    {
        _httpRequestFactory = httpRequestFactories[Constants.IndexKey];
    }

    public async Task<ApiModel> Load(ISourceLoaderConfiguration cfg)
    {
        var model = await _httpRequestFactory.New(cfg.Server)
            .Get(Constants.ApiSourceEndpoint)
            .AsAsync<ApiModel>();

        return model;
    }
}