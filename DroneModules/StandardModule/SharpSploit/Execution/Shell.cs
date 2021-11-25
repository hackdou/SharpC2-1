using System.Diagnostics;
using System.IO;
using System.Text;

namespace Drone.SharpSploit.Execution
{
    public static class Shell
    {
        public static string ExecuteShellCommand(string command)
        {
            return Execute(@"C:\Windows\System32\cmd.exe", $"/c {command}");
        }

        public static string ExecuteRunCommand(string command, string arguments = "")
        {
            return Execute(command, arguments);
        }

        public static string Execute(string command, string arguments = "")
        {
            var output = new StringBuilder();
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            process.OutputDataReceived += (_, args) => { output.AppendLine(args.Data); };
            process.ErrorDataReceived += (_, args) => { output.AppendLine(args.Data); };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            process.Dispose();

            return output.ToString();
        }
    }
}