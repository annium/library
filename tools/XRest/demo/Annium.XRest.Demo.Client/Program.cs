using System;
using Annium.Core.Entrypoint;
using Annium.XRest.Demo.Client;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

Console.WriteLine("Hello from Annium.XRest.Demo.Client");
