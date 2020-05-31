using System;
using System.Threading.Tasks;
using Annium.Core.Mapper;
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

        public Loader(
            IAssemblyLoader assemblyLoader
        )
        {
            _assemblyLoader = assemblyLoader;
        }

        public async Task<ApiModel> Load(
            Uri apiUri,
            string assemblyPath
        )
        {
            var assembly = _assemblyLoader.LoadFromPath(assemblyPath);
            Mapper.AddConfiguration(x => x.ConfigureForTypeViewDeserialization(assembly));

            var view = await Http.Open(apiUri).Get(Constants.ApiSourceEndpoint).AsAsync<ApiView>();
            var model = Mapper.Map<ApiModel>(view);

            return model;
        }
    }
}