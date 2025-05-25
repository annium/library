using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using XSass;
using XSass.Internal;
using XSass.Internal.Components;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, _) = entry;

var configuration = provider.Resolve<Configuration>();
Console.WriteLine($"Sass compilation at: {configuration.Root}");
await provider.Resolve<Crawler>().RunAsync(configuration.Root);
Console.WriteLine("Sass compilation succeed");
