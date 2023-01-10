using System;
using Annium.Core.Entrypoint;
using Xdb;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

Console.WriteLine("Hello from Xdb");