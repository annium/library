using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives.Net;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace XLog.Commands;

internal class ListenCommand : AsyncCommand<ListenCommandConfiguration>, ILogSubject<ListenCommand>
{
    public override string Id => string.Empty;
    public override string Description => "listen";
    public ILogger<ListenCommand> Logger { get; }

    public ListenCommand(
        ILogger<ListenCommand> logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(
        ListenCommandConfiguration cfg,
        CancellationToken ct
    )
    {
        var endpoint = IPEndPointExt.Parse(cfg.Endpoint, 1111);
        this.Log().Info($"Listen at {endpoint} {(cfg.KeepAlive > 0 ? $"with KeepAlive {cfg.KeepAlive}s" : "w/o KeepAlive")}");
        Func<byte[], NetworkStream, Task<int>> receive = cfg.KeepAlive > 0 ? ReceiveKeepAlive : Receive;
        var server = new TcpListener(endpoint);
        server.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 2);
        server.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
        server.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
        server.Start();

        ct.Register(server.Stop);

        // accept single client
        while (!ct.IsCancellationRequested)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(1024);

            TcpClient? client = default!;
            try
            {
                client = await server.AcceptTcpClientAsync();
                this.Log().Debug("Client connected");
                var ns = client.GetStream();

                while (client.Connected)
                {
                    var bytes = await receive(buffer, ns);
                    Console.Write(Encoding.UTF8.GetString(buffer[..bytes]));
                }
            }
            catch (OperationCanceledException)
            {
                this.Log().Debug(nameof(OperationCanceledException));
            }
            catch (ObjectDisposedException e)
            {
                this.Log().Debug($"{nameof(ObjectDisposedException)}: {e}");
            }
            catch (IOException e)
            {
                this.Log().Debug($"{nameof(IOException)}: {e}");
            }
            catch (SocketException e)
            {
                this.Log().Debug($"{nameof(SocketException)}: {e}");
            }
            finally
            {
                pool.Return(buffer);
            }

            if (client is not null)
            {
                client.Close();
                client.Dispose();
            }

            this.Log().Debug("Client disconnected");
        }

        server.Stop();

        async Task<int> ReceiveKeepAlive(byte[] buffer, NetworkStream ns)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(cfg.KeepAlive));

            return await ns.ReadAsync(buffer, 0, buffer.Length, cts.Token);
        }

        async Task<int> Receive(byte[] buffer, NetworkStream ns)
        {
            return await ns.ReadAsync(buffer, 0, buffer.Length, ct);
        }
    }
}

public class ListenCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Endpoint to listen")]
    public string Endpoint { get; set; } = "localhost:1111";

    [Option("k", isRequired: false)]
    [Help("Keep connection alive")]
    public uint KeepAlive { get; set; } = 0;
}