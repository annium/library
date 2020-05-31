using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using XRest.Core.Views;

namespace XRest.Plugins.AspNetCore
{
    internal class XRestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMapper _mapper;
        private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
        private readonly ISerializer<string> _serializer = StringSerializer.Default;

        public XRestMiddleware(
            RequestDelegate next,
            IMapper mapper,
            IApiDescriptionGroupCollectionProvider descriptionProvider
        )
        {
            _next = next;
            _mapper = mapper;
            _descriptionProvider = descriptionProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/.xrest"))
            {
                await _next(context);
                return;
            }

            var apiDescriptions = _descriptionProvider.ApiDescriptionGroups.Items.SelectMany(x => x.Items).ToArray();
            var model = new ApiModelBuilder().Build(apiDescriptions);
            var view = _mapper.Map<ApiView>(model);

            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(_serializer.Serialize(view));
        }
    }
}