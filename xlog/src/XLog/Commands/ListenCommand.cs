using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net;

namespace XLog.Commands;

internal class ListenCommand : AsyncCommand<ListenCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "listen";
    public static string Description => "listen";
    public ILogger Logger { get; }

    public ListenCommand(ILogger logger)
    {
        Logger = logger;
    }

    public override async Task HandleAsync(ListenCommandConfiguration cfg, CancellationToken ct)
    {
        var endpoint = IPEndPointExt.Parse(cfg.Endpoint, 1111);
        this.Info<IPEndPoint, string>(
            "Listen at {endpoint} {keepAlive}",
            endpoint,
            cfg.KeepAlive > 0 ? $"with KeepAlive {cfg.KeepAlive}s" : "w/o KeepAlive"
        );
        Func<byte[], NetworkStream, Task<int>> receive = cfg.KeepAlive > 0 ? ReceiveKeepAliveAsync : ReceiveAsync;
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

            TcpClient? client = null;
            try
            {
                client = await server.AcceptTcpClientAsync();
                this.Debug("Client connected");
                var ns = client.GetStream();

                while (client.Connected)
                {
                    var bytes = await receive(buffer, ns);
                    Console.Write(Encoding.UTF8.GetString(buffer[..bytes]));
                }
            }
            catch (OperationCanceledException)
            {
                this.Debug(nameof(OperationCanceledException));
            }
            catch (ObjectDisposedException e)
            {
                this.Debug("{type}: {exception}", nameof(ObjectDisposedException), e);
            }
            catch (IOException e)
            {
                this.Debug("{type}: {exception}", nameof(IOException), e);
            }
            catch (SocketException e)
            {
                this.Debug("{type}: {exception}", nameof(SocketException), e);
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

            this.Debug("Client disconnected");
        }

        server.Stop();

        async Task<int> ReceiveKeepAliveAsync(byte[] buffer, NetworkStream ns)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(cfg.KeepAlive));

            return await ns.ReadAsync(buffer, 0, buffer.Length, cts.Token);
        }

        async Task<int> ReceiveAsync(byte[] buffer, NetworkStream ns)
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
