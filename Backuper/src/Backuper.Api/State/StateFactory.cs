using System;
using System.Collections.Generic;
using System.Linq;
using Backuper.Api.Config;
using Backuper.Connection.Abstract;
using Backuper.Notification.Abstract;
using Backuper.Storage;

namespace Backuper.Api.State;

public class StateFactory
{
    private readonly Configuration config;

    private readonly ConnectionFactory connectionFactory;

    private readonly StorageFactory storageFactory;

    private readonly ChannelFactory channelFactory;

    public StateFactory(
        Configuration config,
        ConnectionFactory connectionFactory,
        StorageFactory storageFactory,
        ChannelFactory channelFactory
    )
    {
        this.config = config;
        this.connectionFactory = connectionFactory;
        this.storageFactory = storageFactory;
        this.channelFactory = channelFactory;
    }

    public State GetState()
    {
        var servers = ResolveAll(config.Servers, ResolveServer);

        return new State(servers);
    }

    private Server ResolveServer(string name, ServerConfiguration cfg)
    {
        var connection = connectionFactory.CreateConnection(cfg.Connection);
        var plans = ResolveAll(cfg.Plans, ResolvePlan);

        return new Server(name, connection, plans);
    }

    private Plan ResolvePlan(string name, PlanConfiguration cfg)
    {
        var storage = storageFactory.CreateStorage(cfg.Storage);
        var channels = ResolveAll(cfg.Notifications, (n, c) => channelFactory.CreateChannel(c));

        return new Plan(name, storage, cfg.Interval, cfg.Capacity, channels);
    }

    private IReadOnlyDictionary<string, R> ResolveAll<C, R>(
        IDictionary<string, C> config,
        Func<string, C, R> resolve
    ) => config.ToDictionary(
        pair => pair.Key,
        pair => resolve(pair.Key, pair.Value)
    );
}