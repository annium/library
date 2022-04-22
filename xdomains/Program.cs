using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using xdomains;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

Commander.Run<xdomains.Commands.Group>(provider, args, ct);