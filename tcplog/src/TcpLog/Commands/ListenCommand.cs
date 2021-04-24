using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace TcpLog.Commands
{
    internal class ListenCommand : AsyncCommand<ListenCommandConfiguration>
    {
        private readonly ILogger<ListenCommand> _logger;
        public override string Id { get; } = string.Empty;
        public override string Description { get; } = "listen";

        public ListenCommand(
            ILogger<ListenCommand> logger
        )
        {
            _logger = logger;
        }

        public override async Task HandleAsync(
            ListenCommandConfiguration cfg,
            CancellationToken ct
        )
        {
            var endpoint = IPEndPointExt.Parse(cfg.Endpoint, 1111);
            _logger.Info($"Listen at {endpoint}");
            var server = new TcpListener(endpoint);
            server.Start();

            ct.Register(server.Stop);

            // accept single client
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var client = await server.AcceptTcpClientAsync();
                    _logger.Debug("Client connected");
                    var ns = client.GetStream();

                    var pool = ArrayPool<byte>.Shared;
                    var buffer = pool.Rent(1024);
                    while (client.Connected)
                    {
                        await ns.ReadAsync(buffer, ct);
                        Console.Write(Encoding.UTF8.GetString(buffer));
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (IOException)
                {
                }
                catch (SocketException)
                {
                }

                _logger.Debug("Client disconnected");
            }

            server.Stop();
        }
    }

    public class ListenCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Endpoint to listen")]
        public string Endpoint { get; set; } = "localhost:1111";
    }
}