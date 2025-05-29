using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Annium.XRest;
using Group = Annium.XRest.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

await Commander.RunAsync<Group>(provider, args, ct);
