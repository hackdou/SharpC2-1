namespace SharpC2.API.V1.Responses
{
    public class DroneTaskResponse
    {
        public string TaskGuid { get; set; }
        public string Command { get; set; }
        public TaskStatus Status { get; set; }
        public byte[] Result { get; set; }
        
        public enum TaskStatus
        {
            Pending,
            Tasked,
            Running,
            Complete,
            Cancelled,
            Aborted
        }
    }
}