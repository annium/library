using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Types;
using Annium.Serialization.Abstractions;
using Annium.XRest.Sources.AspNetCore.Internal.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Constants = Annium.XRest.Core.Constants;

namespace Annium.XRest.Sources.AspNetCore;

internal class XRestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
    private readonly IModelMapper _modelMapper;
    private readonly IMapperConfig _mapperConfig;
    private readonly ISerializer<string> _serializer;
    private readonly Lazy<string> _description;

    public XRestMiddleware(
        RequestDelegate next,
        IServiceProvider sp,
        IApiDescriptionGroupCollectionProvider descriptionProvider,
        IModelMapper modelMapper,
        IMapperConfig mapperConfig
    )
    {
        _next = next;
        _descriptionProvider = descriptionProvider;
        _modelMapper = modelMapper;
        _mapperConfig = mapperConfig;
        var serializerKey = SerializerKey.Create(Constants.IndexKey, MediaTypeNames.Application.Json);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
        _description = new Lazy<string>(BuildApiDescription, isThreadSafe: true);
    }

    public async Task InvokeAsync(HttpContext context)
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
        var mappingContext = new MappingContext(_modelMapper, _mapperConfig);
        var model = ApiModelBuilder.Build(apiDescriptions, mappingContext);
        var raw = _serializer.Serialize(model);

        return raw;
    }
}
