using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using xdomains;
using Group = xdomains.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

Commander.Run<Group>(provider, args, ct);
