using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Integrations.Social.Telegram;
using Annium.Integrations.Social.Telegram.Demo;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

await entry.Provider.ResolveKeyed<ITelegramBotHost>("demo").RunAsync(entry.Ct);
