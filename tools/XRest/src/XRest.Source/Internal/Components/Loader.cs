using System.Threading.Tasks;
using Annium.Net.Http;
using XRest.Core;
using XRest.Core.Models;
using XRest.Source.Components;

namespace XRest.Source.Internal.Components;

internal class Loader : ILoader
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public Loader(
        IHttpRequestFactory httpRequestFactory
    )
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public async Task<ApiModel> Load(ISourceLoaderConfiguration cfg)
    {
        var model = await _httpRequestFactory.New(cfg.Server)
            .Get(Constants.ApiSourceEndpoint)
            .AsAsync<ApiModel>();

        return model;
    }
}