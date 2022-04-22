using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Logging.Abstractions;
using XSass;
using XSass.Internal;
using XSass.Internal.Components;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, _) = entry;

var configuration = provider.Resolve<Configuration>();
var logSubject = provider.Resolve<ILogSubject<Program>>();
logSubject.Log().Info($"Sass compilation at: {configuration.Root}");
await provider.Resolve<Crawler>().Run(configuration.Root);
logSubject.Log().Info("Sass compilation succeed");