using Annium.Core.Entrypoint;
using Annium.DocLint;
using Annium.Extensions.Arguments;
using Group = Annium.DocLint.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

await Commander.RunAsync<Group>(provider, args, ct);
