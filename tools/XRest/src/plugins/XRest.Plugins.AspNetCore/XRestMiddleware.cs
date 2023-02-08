using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using XRest.Plugins.AspNetCore.Internal.Components;
using Constants = XRest.Core.Constants;

namespace XRest.Plugins.AspNetCore;

internal class XRestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMapper _mapper;
    private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
    private readonly ISerializer<string> _serializer;
    private readonly Lazy<string> _description;

    public XRestMiddleware(
        RequestDelegate next,
        IMapper mapper,
        IApiDescriptionGroupCollectionProvider descriptionProvider,
        IIndex<SerializerKey, ISerializer<string>> serializers
    )
    {
        _next = next;
        _mapper = mapper;
        _descriptionProvider = descriptionProvider;
        _serializer = serializers[SerializerKey.Create(Constants.IndexKey, MediaTypeNames.Application.Json)];
        _description = new Lazy<string>(BuildApiDescription, isThreadSafe: true);
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.Equals($"/{Constants.ApiSourceEndpoint}"))
        {
            await _next(context);
            return;
        }

        context.Response.ContentType = MediaTypeNames.Application.Json;
        var description = _description.Value;
        await context.Response.WriteAsync(description);
    }

    private string BuildApiDescription()
    {
        var apiDescriptions = _descriptionProvider.ApiDescriptionGroups.Items.SelectMany(x => x.Items).ToArray();
        var model = ApiModelBuilder.Build(apiDescriptions);
        // var view = _mapper.Map<ApiView>(model);
        var raw = _serializer.Serialize(model);

        return raw;
    }
}