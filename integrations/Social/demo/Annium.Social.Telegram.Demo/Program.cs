using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Social.Telegram;
using Annium.Social.Telegram.Demo;

await using var entry = await Entrypoint.Default.UseServicePack<ServicePack>().SetupAsync();

await entry.Provider.ResolveKeyed<ITelegramBotHost>("demo").RunAsync(entry.Ct);
