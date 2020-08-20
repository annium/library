using System;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Net.Http;
using XRest.Core;
using XRest.Core.Components;
using XRest.Core.Models;
using XRest.Core.Views;
using XRest.Core.Views.Profiles;

namespace XRest.Sources.Api.Components.Internal
{
    internal class Loader : ILoader
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IHttpRequestFactory _httpRequestFactory;
        private readonly IMapper _mapper;
        private readonly IMapBuilder _mapBuilder;

        public Loader(
            IAssemblyLoader assemblyLoader,
            IHttpRequestFactory httpRequestFactory,
            IMapper mapper,
            IMapBuilder mapBuilder
        )
        {
            _assemblyLoader = assemblyLoader;
            _httpRequestFactory = httpRequestFactory;
            _mapper = mapper;
            _mapBuilder = mapBuilder;
        }

        public async Task<ApiModel> Load(
            Uri apiUri,
            string assemblyPath
        )
        {
            var assembly = _assemblyLoader.LoadFromPath(assemblyPath);
            _mapBuilder.AddProfile(x => x.ConfigureForTypeViewDeserialization(assembly));

            var view = await _httpRequestFactory.New(apiUri).Get(Constants.ApiSourceEndpoint).AsAsync<ApiView>();
            var model = _mapper.Map<ApiModel>(view);

            return model;
        }
    }
}