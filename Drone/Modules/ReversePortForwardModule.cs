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
            var start = new Command();
            start.Arguments.Add(new Command.Argument());
            start.Arguments.Add(new Command.Argument("", false));
            start.Arguments.Add(new Command.Argument("", false));

            var stop = new Command();
            stop.Arguments.Add(new Command.Argument("", false));

            var list = new Command();
            var purge = new Command();

            var inbound = new Command() {Visible = false};

            Commands.Add(start);
            Commands.Add(stop);
            Commands.Add(list);
            Commands.Add(purge);
            Commands.Add(inbound);
        }

        
    }

    
}