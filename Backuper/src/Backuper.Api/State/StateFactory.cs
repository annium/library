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
    private readonly Configuration _config;

    private readonly ConnectionFactory _connectionFactory;

    private readonly StorageFactory _storageFactory;

    private readonly ChannelFactory _channelFactory;

    public StateFactory(
        Configuration config,
        ConnectionFactory connectionFactory,
        StorageFactory storageFactory,
        ChannelFactory channelFactory
    )
    {
        _config = config;
        _connectionFactory = connectionFactory;
        _storageFactory = storageFactory;
        _channelFactory = channelFactory;
    }

    public State GetState()
    {
        var servers = ResolveAll(_config.Servers, ResolveServer);

        return new State(servers);
    }

    private Server ResolveServer(string name, ServerConfiguration cfg)
    {
        var connection = _connectionFactory.CreateConnection(cfg.Connection);
        var plans = ResolveAll(cfg.Plans, ResolvePlan);

        return new Server(name, connection, plans);
    }

    private Plan ResolvePlan(string name, PlanConfiguration cfg)
    {
        var storage = _storageFactory.CreateStorage(cfg.Storage);
        var channels = ResolveAll(cfg.Notifications, (n, c) => _channelFactory.CreateChannel(c));

        return new Plan(name, storage, cfg.Interval, cfg.Capacity, channels);
    }

    private IReadOnlyDictionary<string, TR> ResolveAll<TC, TR>(
        IDictionary<string, TC> config,
        Func<string, TC, TR> resolve
    ) => config.ToDictionary(pair => pair.Key, pair => resolve(pair.Key, pair.Value));
}
