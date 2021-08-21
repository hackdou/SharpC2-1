using System;
using System.Collections.Generic;

namespace SharpC2.Models
{
    public class Drone : SharpSploitResult
    {
        public string Guid { get; set; }
        public string Address { get; set; }
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Process { get; set; }
        public int Pid { get; set; }
        public DroneIntegrity Integrity { get; set; }
        public DroneArch Arch { get; set; }
        public DateTime LastSeen { get; set; }

        public List<DroneModule> Modules { get; set; }

        public double LastSeenSeconds
        {
            get
            {
                var time = (DateTime.UtcNow - LastSeen).TotalSeconds;
                return Math.Round(time, 2);
            }
        }
        
        public enum DroneIntegrity
        {
            Medium,
            High,
            SYSTEM
        }

        public enum DroneArch
        {
            x86,
            x64
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Guid", Value = Guid},
                new() {Name = "Address", Value = Address},
                new() {Name = "Hostname", Value = Hostname},
                new() {Name = "Username", Value = Username},
                new() {Name = "Process", Value = Process},
                new() {Name = "PID", Value = Pid},
                new() {Name = "Integrity", Value = Integrity},
                new() {Name = "Arch", Value = Arch},
                new() {Name = "LastSeen", Value = $"{LastSeenSeconds}s"}
            };
    }
}