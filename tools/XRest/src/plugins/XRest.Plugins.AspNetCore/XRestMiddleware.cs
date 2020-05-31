using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using XRest.Core.Infrastructure.JsonConverters;

namespace XRest.Plugins.AspNetCore
{
    internal class XRestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;

        private readonly ISerializer<string> _serializer = StringSerializer.Configure(opts =>
        {
            opts.Converters.Add(new TypeJsonConverter());
            opts.WriteIndented = true;
            opts.ConfigureDefault();
        });

        public XRestMiddleware(
            RequestDelegate next,
            IApiDescriptionGroupCollectionProvider descriptionProvider
        )
        {
            _next = next;
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
            var apiModel = new ApiModelBuilder().Build(apiDescriptions);

            await context.Response.WriteAsync(_serializer.Serialize(apiModel));
        }
    }
}