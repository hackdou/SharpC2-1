using System;
using System.Collections.Generic;
using System.IO;

using Drone.SharpSploit.Generic;

namespace Drone.SharpSploit.Enumeration
{
    public static class Host
    {
        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
        
        public static void ChangeCurrentDirectory(string directory)
        {
            Directory.SetCurrentDirectory(directory);
        }
        
        public static SharpSploitResultList<FileSystemEntryResult> GetDirectoryListing(string path)
        {
            var results = new SharpSploitResultList<FileSystemEntryResult>();
            
            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dir);
                
                results.Add(new FileSystemEntryResult
                {
                    Name = dirInfo.FullName,
                    Length = 0,
                    CreationTimeUtc = dirInfo.CreationTimeUtc,
                    LastAccessTimeUtc = dirInfo.LastAccessTimeUtc,
                    LastWriteTimeUtc = dirInfo.LastWriteTimeUtc
                });
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(file);
                
                results.Add(new FileSystemEntryResult
                {
                    Name = fileInfo.FullName,
                    Length = fileInfo.Length,
                    CreationTimeUtc = fileInfo.CreationTimeUtc,
                    LastAccessTimeUtc = fileInfo.LastAccessTimeUtc,
                    LastWriteTimeUtc = fileInfo.LastWriteTimeUtc
                });
            }

            return results;
        }
    }
    
    public sealed class FileSystemEntryResult : SharpSploitResult
    {
        public string Name { get; set; }
        public long Length { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Name", Value = Name},
                new() {Name = "Length", Value = Length},
                new() {Name = "CreationTimeUtc", Value = CreationTimeUtc},
                new() {Name = "LastAccessTimeUtc", Value = LastAccessTimeUtc},
                new() {Name = "LastWriteTimeUtc", Value = LastWriteTimeUtc}
            };
    }
}