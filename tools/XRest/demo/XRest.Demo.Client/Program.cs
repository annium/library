using System;
using Annium.Core.Entrypoint;
using XRest.Demo.Client;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

Console.WriteLine("Hello from XRest.Demo.Client");
