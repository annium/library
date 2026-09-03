using System;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Annium.XRest;
using Group = Annium.XRest.Commands.Group;

await using var entry = await Entrypoint.Default.UseServicePack<ServicePack>().SetupAsync();

var (provider, ct) = entry;

try
{
    await Commander.RunAsync<Group>(provider, args, ct);
    return 0;
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    return 1;
}
