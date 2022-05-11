using MvvmHelpers;

using System.Text;

namespace Client.Models;

public class DroneTaskRecord : ObservableObject
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public string DroneFunction { get; set; }
    public string CommandAlias { get; set; }
    public string[] Parameters { get; set; }
    public string ArtefactPath { get; set; }
    public DateTime StartTime { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime EndTime { get; set; }
    public byte[] Result { get; set; }

    public string ResultText
    {
        get
        {
            return Encoding.UTF8.GetString(Result);
        }
    }

    public string DisplayCommand
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append(DroneFunction);

            if (!string.IsNullOrWhiteSpace(ArtefactPath))
                sb.Append($" {ArtefactPath}");

            if (Parameters is not null && Parameters.Length > 0)
                sb.Append($" {string.Join(" ", Parameters)}");

            return sb.ToString().TrimEnd();
        }
    }

    public enum TaskStatus : int
    {
        Pending = 0,
        Tasked = 1,
        Running = 2,
        Complete = 3,
        Aborted = 4
    }
}