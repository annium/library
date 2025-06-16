using System;
using Annium.Core.Entrypoint;
using Annium.Integrations.Social.Telegram.Demo.Obsolete;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

Console.WriteLine("Hello from Demo.Bots.Telegram");
