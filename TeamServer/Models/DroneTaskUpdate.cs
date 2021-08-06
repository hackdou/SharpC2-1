using System;

namespace TeamServer.Models
{
    [Serializable]
    public class DroneTaskUpdate
    {
        public string TaskGuid { get; set; }
        public string ServerModule { get; set; }
        public TaskStatus Status { get; set; }
        public byte[] Result { get; set; }

        public enum TaskStatus : int
        {
            Pending = 0,
            Tasked = 1,
            Running = 2,
            Complete = 3,
            Cancelled = 4,
            Aborted = 5
        }
    }
}