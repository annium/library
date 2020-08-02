using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using NodaTime.Xml;
using XRest.Core.Views;

namespace XRest.Plugins.AspNetCore
{
    internal class XRestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMapper _mapper;
        private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
        private readonly ISerializer<string> _serializer;

        public XRestMiddleware(
            RequestDelegate next,
            ITypeManager typeManager,
            IMapper mapper,
            IApiDescriptionGroupCollectionProvider descriptionProvider
        )
        {
            _next = next;
            _mapper = mapper;
            _descriptionProvider = descriptionProvider;
            _serializer = StringSerializer.Configure(opts => opts
                .ConfigureDefault(typeManager)
                .ConfigureForOperations()
                .ConfigureForNodaTime(XmlSerializationSettings.DateTimeZoneProvider)
            );
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