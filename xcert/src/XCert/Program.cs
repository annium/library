using System;
using Annium.Core.Entrypoint;
using XCert;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

Console.WriteLine("When have time, this will be certs updating daemon");
