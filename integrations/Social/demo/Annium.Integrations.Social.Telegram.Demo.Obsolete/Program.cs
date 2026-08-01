using System;
using Annium.Core.Entrypoint;
using Annium.Integrations.Social.Telegram.Demo.Obsolete;

await using var entry = await Entrypoint.Default.UseServicePack<ServicePack>().SetupAsync();

Console.WriteLine("Hello from Demo.Bots.Telegram");
