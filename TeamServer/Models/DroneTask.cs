using System;
using System.Text.Json.Serialization;

namespace TeamServer.Models
{
    [Serializable]
    public class DroneTask
    {
        public string TaskGuid { get; set; }
        public string Module { get; set; }
        public string Command { get; set; }
        public string[] Arguments { get; set; }
        public byte[] Artefact { get; set; }
        
        [JsonIgnore]
        public TaskStatus Status { get; set; }
        
        [JsonIgnore]
        public byte[] Result { get; set; }

        public DroneTask(string module, string command)
        {
            Module = module;
            Command = command;
            TaskGuid = Guid.NewGuid().ToShortGuid();
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

        public void UpdateStatus(TaskStatus status)
            => Status = status;

        public void UpdateResult(byte[] result)
        {
            if (result is null || result.Length == 0) return;
            
            if (Result is null)
            {
                Result = result;
                return;
            }

            var current = Result;
            Array.Resize(ref current, current.Length + result.Length);
            Buffer.BlockCopy(result, 0, current, Result.Length, result.Length);
            Result = current;
        }
    }
}