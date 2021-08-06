using System;
using System.Runtime.Serialization;

namespace Drone.Models
{
    [DataContract]
    public class DroneTaskUpdate
    {
        [DataMember (Name = "taskGuid")]
        public string TaskGuid { get; set; }
        
        [DataMember (Name = "serverModule")]
        public string ServerModule { get; set; }
        
        [DataMember (Name = "status")]
        public TaskStatus Status { get; set; }
        
        [DataMember (Name = "result")]
        public string Result { get; set; }

        public DroneTaskUpdate(string taskGuid, TaskStatus status, byte[] result = null)
        {
            TaskGuid = taskGuid;
            ServerModule = "core";
            Status = status;
            Result = result == null ? null : Convert.ToBase64String(result);
        }

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