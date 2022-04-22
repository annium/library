using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using XRest;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

Commander.Run<XRest.Commands.Group>(provider, args, ct);