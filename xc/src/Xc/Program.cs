using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Xc;
using Group = Xc.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

await Commander.RunAsync<Group>(provider, args, ct);
