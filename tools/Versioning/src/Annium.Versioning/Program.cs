using System;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Annium.Versioning;
using Group = Annium.Versioning.Commands.Group;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

try
{
    await Commander.RunAsync<Group>(provider, args, ct);
    return 0;
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
    return 1;
}
