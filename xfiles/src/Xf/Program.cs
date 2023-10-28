using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Xf;
using Group = Xf.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

Commander.Run<Group>(provider, args, ct);
