using System;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using XRest.Core.Models;

namespace XRest.Sources.Components.Internal
{
    internal class Loader : ILoader
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Loader> _logger;

        public Loader(
            IServiceProvider serviceProvider,
            ILogger<Loader> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ApiModel> Load(ISourceLoaderConfiguration cfg)
        {
            if (cfg.Server is null)
                return LoadFromAssembly(cfg);

            return await LoadFromApi(cfg);
        }

        private async Task<ApiModel> LoadFromApi(ISourceLoaderConfiguration cfg)
        {
            _logger.Info($"Loading from server '{cfg.Server}'");

            var loader = _serviceProvider.GetRequiredService<Sources.Api.Components.ILoader>();

            return await loader.Load(cfg.Server!, cfg.Assembly);
        }

        private ApiModel LoadFromAssembly(ISourceLoaderConfiguration cfg)
        {
            _logger.Info($"Loading from assembly '{cfg.Assembly}'");

            var loader = _serviceProvider.GetRequiredService<Sources.Assembly.Components.ILoader>();

            return loader.Load(cfg.Assembly);
        }
    }
}