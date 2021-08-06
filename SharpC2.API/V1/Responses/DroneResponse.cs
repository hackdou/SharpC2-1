using System;
using System.Collections.Generic;

namespace SharpC2.API.V1.Responses
{
    public class DroneResponse
    {
        public string Guid { get; set; }
        public string Address { get; set; }
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Process { get; set; }
        public int Pid { get; set; }
        public string Arch { get; set; }
        public DateTime LastSeen { get; set; }

        public IEnumerable<DroneModuleResponse> Modules { get; set; }
    }
}