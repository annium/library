using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using XLog;
using Group = XLog.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

await Commander.RunAsync<Group>(provider, args, ct);
