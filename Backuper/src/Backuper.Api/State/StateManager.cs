using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Jobs;
using Backuper.Api.Config;

namespace Backuper.Api.State
{
    public class StateManager
    {
        private readonly Configuration config;

        private readonly Connection.Abstract.ConnectionFactory connectionFactory;

        private readonly Storage.Abstract.StorageFactory storageFactory;

        private readonly Notification.Abstract.ChannelFactory channelFactory;

        private readonly IIntervalResolver intervalResolver;

        public StateManager(
            Configuration config,
            Connection.Abstract.ConnectionFactory connectionFactory,
            Storage.Abstract.StorageFactory storageFactory,
            Notification.Abstract.ChannelFactory channelFactory,
            IIntervalResolver intervalResolver
        )
        {
            this.config = config;
            this.connectionFactory = connectionFactory;
            this.storageFactory = storageFactory;
            this.channelFactory = channelFactory;
            this.intervalResolver = intervalResolver;
        }

        public async Task<State> GetState()
        {
            var storages = await GetAllAsync(config.Storages, p => storageFactory.GetStorageAsync(p.Value));
            var channels = await GetAllAsync(config.Notifications, p => channelFactory.GetChannelAsync(p.Value));

            var servers = await GetAllAsync(config.Servers, p => GetServerAsync(storages, channels, p.Key, p.Value));

            return new State(servers);
        }

        private async Task<Server> GetServerAsync(
            IReadOnlyDictionary<string, Storage.Abstract.Storage> storages,
            IReadOnlyDictionary<string, Notification.Abstract.Channel> channels,
            string name,
            ServerConfiguration cfg
        )
        {
            var connection = await connectionFactory.GetConnectionAsync(cfg.Connection);
            var plans = cfg.Plans.ToDictionary(
                p => p.Key,
                p => ResolvePlan(name, p.Key, p.Value, storages, channels)
            );

            return new Server(connection, plans);
        }

        private Plan ResolvePlan(
            string server,
            string name,
            PlanConfiguration cfg,
            IReadOnlyDictionary<string, Storage.Abstract.Storage> storages,
            IReadOnlyDictionary<string, Notification.Abstract.Channel> channels
        ) => new Plan(
            storages.FirstOrDefault(p => p.Key == cfg.Storage).Value ??
            throw new InvalidOperationException($"Can't resolve storage {cfg.Storage} of plan {server}.{name}"),
                intervalResolver.GetMatcher(cfg.Interval),
                cfg.Notifications
                .Select(c => channels.FirstOrDefault(p => p.Key == c).Value ??
                    throw new InvalidOperationException($"Can't resolve notification channel {c} of plan {server}.{name}"))
                .ToArray()
        );

        private async Task<IReadOnlyDictionary<string, R>> GetAllAsync<C, R>(
            IReadOnlyDictionary<string, C> config, Func<KeyValuePair<string, C>, Task<R>> resolveAsync
        )
        {
            return (await Task.WhenAll(config.Select(async pair =>
                {
                    var result = await resolveAsync(pair);

                    return new KeyValuePair<string, R>(pair.Key, result);
                })))
                .ToDictionary(p => p.Key, p => p.Value);
        }
    }
}