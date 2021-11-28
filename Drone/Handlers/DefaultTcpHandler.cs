using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Handlers
{
    public class DefaultTcpHandler : Handler
    {
        public override string Name { get; } = "default-tcp";

        private readonly HandlerMode _mode;
        private readonly string _target;
        
        private CancellationTokenSource _tokenSource;

        private TcpListener _tcpListener;
        private TcpClient _tcpClient;

        public DefaultTcpHandler()
        {
            _mode = HandlerMode.Server;
        }

        public DefaultTcpHandler(string target)
        {
            _mode = HandlerMode.Client;
            _target = target;
        }

        public override async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            switch (_mode)
            {
                case HandlerMode.Client:
                    _tcpClient = new TcpClient();

                    IPAddress target;

                    if (!IPAddress.TryParse(_target, out target))
                    {
                        // do a DNS lookup
                        var dns = await Dns.GetHostAddressesAsync(_target);
                        
                        // take the first IPv4
                        target = dns.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                    }
                    
                    // blocks until connected
                    await _tcpClient.ConnectAsync(target, BindPort);
                    await RunReadWriteLoop(_tcpClient.GetStream());
                    break;
                
                case HandlerMode.Server:

                    var ipAddress = LocalhostOnly ? IPAddress.Loopback : IPAddress.Any;
                    var endPoint = new IPEndPoint(ipAddress, BindPort);

                    _tcpListener = new TcpListener(endPoint);
                    _tcpListener.Start(100);
                    
                    // blocks until client connects
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    await RunReadWriteLoop(tcpClient.GetStream());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task RunReadWriteLoop(Stream stream)
        {
            var readTask = Task.Run(async () =>
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    var inbound = await ReadFromStream(stream);

                    if (inbound is null || inbound.Length == 0)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    var envelopes = inbound.Deserialize<MessageEnvelope[]>();
                    
                    if (envelopes.Any())
                        foreach (var envelope in envelopes)
                            InboundQueue.Enqueue(envelope);

                    await Task.Delay(1000);
                }

            }, _tokenSource.Token);

            var writeTask = Task.Run(async () =>
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    if (OutboundQueue.IsEmpty)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    var outbound = GetOutboundQueue().ToArray();
                    var raw = outbound.Serialize();
                    await WriteToStream(stream, raw);
                }

            }, _tokenSource.Token);

            await Task.WhenAll(readTask, writeTask);
        }

        private static async Task<byte[]> ReadFromStream(Stream stream)
        {
            const int readSize = 1024;

            using var ms = new MemoryStream();

            while (true)
            {
                var buf = new byte[readSize];
                var read = await stream.ReadAsync(buf, 0, buf.Length);
                await ms.WriteAsync(buf, 0, read);

                if (read < readSize) break;
            }

            return ms.ToArray();
        }

        private static async Task WriteToStream(Stream stream, byte[] data)
        {
            await stream.WriteAsync(data, 0, data.Length);
        }

        public override void Stop()
        {
            _tokenSource.Cancel();

            switch (_mode)
            {
                case HandlerMode.Client:
                    _tcpClient.Dispose();
                    break;
                
                case HandlerMode.Server:
                    _tcpListener.Stop();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool LocalhostOnly => bool.Parse("false");
        private static int BindPort => int.Parse("4444");
    }
}