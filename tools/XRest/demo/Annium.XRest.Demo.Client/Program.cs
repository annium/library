using System;
using Annium.Core.Entrypoint;
using Annium.XRest.Demo.Client;

await using var entry = await Entrypoint.Default.UseServicePack<ServicePack>().SetupAsync();

Console.WriteLine("Hello from Annium.XRest.Demo.Client");
