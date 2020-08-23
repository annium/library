using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Core.Runtime.Loader;
using Annium.Net.Http;
using XRest.Core;
using XRest.Core.Models;
using XRest.Core.Views;
using XRest.Core.Views.Profiles;

namespace XRest.Sources.Api.Components.Internal
{
    internal class Loader : ILoader
    {
        private readonly IAssemblyLoaderBuilder _assemblyLoaderBuilder;
        private readonly IHttpRequestFactory _httpRequestFactory;
        private readonly IMapper _mapper;
        private readonly IMapBuilder _mapBuilder;

        public Loader(
            IAssemblyLoaderBuilder assemblyLoaderBuilder,
            IHttpRequestFactory httpRequestFactory,
            IMapper mapper,
            IMapBuilder mapBuilder
        )
        {
            _assemblyLoaderBuilder = assemblyLoaderBuilder;
            _httpRequestFactory = httpRequestFactory;
            _mapper = mapper;
            _mapBuilder = mapBuilder;
        }

        public async Task<ApiModel> Load(
            Uri apiUri,
            string assemblyPath
        )
        {
            var loader = _assemblyLoaderBuilder.UseFileSystemLoader(Path.GetDirectoryName(assemblyPath)!).Build();
            var name = Path.GetFileNameWithoutExtension(assemblyPath);
            var assembly = loader.Load(name);
            _mapBuilder.AddProfile(x => x.ConfigureForTypeViewDeserialization(assembly));

            var view = await _httpRequestFactory.New(apiUri).Get(Constants.ApiSourceEndpoint).AsAsync<ApiView>();
            var model = _mapper.Map<ApiModel>(view);

            return model;
        }
    }
}