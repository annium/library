using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using MessageBus.Proxy;
using NetMQ;
using NetMQ.Sockets;

await using var entry = Entrypoint.Default.UseServicePack<ServicePack>().Setup();

var (provider, ct) = entry;

var cfg = provider.Resolve<Configuration>();
Console.WriteLine($"Start proxy with PUB {cfg.PubEndpoint} / SUB {cfg.SubEndpoint}");

using var subscriber = new XSubscriberSocket();
subscriber.Bind(cfg.PubEndpoint);

using var publisher = new XPublisherSocket();
publisher.Bind(cfg.SubEndpoint);

var proxy = new Proxy(subscriber, publisher);
ct.Register(proxy.Stop);
proxy.Start();
