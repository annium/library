using System;
using Annium.Core.Entrypoint;
using XLink;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

Console.WriteLine("Hello from XLink");
