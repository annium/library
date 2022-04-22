using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Primitives.Threading;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Logging.Abstractions;
using MessageBus.Sink;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

var logSubject = provider.Resolve<ILogSubject<Program>>();
var socket = provider.Resolve<IMessageBusSocket>();

var cfg = provider.Resolve<EndpointsConfiguration>();
Console.WriteLine($"Start sink with PUB {cfg.PubEndpoint} / SUB {cfg.SubEndpoint}");

socket.Subscribe(x => logSubject.Log().Info(x));

await ct;