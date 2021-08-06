namespace SharpC2.Models
{
    public class DroneTask
    {
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