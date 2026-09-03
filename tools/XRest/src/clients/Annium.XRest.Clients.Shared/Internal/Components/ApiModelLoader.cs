using System;
using System.Threading;
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

    public async Task<ApiModel> LoadAsync(ISourceLoaderConfiguration cfg, CancellationToken ct)
    {
        var model = await _httpRequestFactory
            .New(cfg.Server)
            .Get(Constants.ApiSourceEndpoint)
            // AsAsync swallows a failed response and returns default, which would surface much later
            // as an unhelpful "value is null" — report the status against the URI instead
            .Intercept(
                async (next, request) =>
                {
                    var response = await next();
                    if (!response.IsSuccess)
                        throw new InvalidOperationException(
                            $"Failed to load API model from {request.Uri}: {(int)response.StatusCode} {response.StatusCode}"
                        );

                    return response;
                }
            )
            .AsAsync<ApiModel>(ct);

        return model.NotNull($"API model served by {cfg.Server} could not be parsed");
    }
}
