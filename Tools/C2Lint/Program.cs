using System;
using System.IO;
using System.Threading.Tasks;

using TeamServer.Models;

namespace C2Lint
{
    public class Program
    {
        private static C2Profile _profile;

        private static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                await Console.Error.WriteLineAsync("Path to YAML file required.");
                return;
            }

            var yaml = await File.ReadAllTextAsync(args[0]);
            var deserializer = new YamlDotNet.Serialization.Deserializer();

            try
            {
                _profile = deserializer.Deserialize<C2Profile>(yaml);
                RunChecks();
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync("Error during deserialization.");
                await Console.Error.WriteLineAsync(e.Message);
                await Console.Error.WriteLineAsync(e.StackTrace);
            }
        }

        private static void RunChecks()
        {
            StageChecks();
            PostExploitationChecks();
            ProcessInjectionChecks();
            
            Console.WriteLine();
        }

        private static void StageChecks()
        {
            var checks = new StageChecks(_profile.Stage);
            
            Console.WriteLine("Stage Block");
            Console.WriteLine("===========");
            
            Console.WriteLine("Dll Export: {0}", _profile.Stage.DllExport);
            checks.CheckDllExport();
            
            Console.WriteLine();
        }

        private static void PostExploitationChecks()
        {
            var checks = new PostExploitationChecks(_profile.PostExploitation);
            
            Console.WriteLine("Post Exploitation Block");
            Console.WriteLine("=======================");
            
            Console.WriteLine("Bypass AMSI: {0}", _profile.PostExploitation.BypassAmsi);
            Console.WriteLine("Bypass ETW: {0}", _profile.PostExploitation.BypassEtw);

            Console.WriteLine("SpawnTo: {0}", _profile.PostExploitation.SpawnTo);
            checks.CheckSpawnTo();

            Console.WriteLine("AppDomain: {0}", _profile.PostExploitation.AppDomain);
            checks.CheckAppDomain();
            
            Console.WriteLine();
        }

        private static void ProcessInjectionChecks()
        {
            var checks = new ProcessInjectionChecks(_profile.ProcessInjection);
            
            Console.WriteLine("Process Injection Block");
            Console.WriteLine("=======================");
            
            Console.WriteLine("Allocation: {0}", _profile.ProcessInjection.Allocation);
            checks.CheckAllocation();
            
            Console.WriteLine("Execution: {0}", _profile.ProcessInjection.Execution);
            checks.CheckExecution();
            
            Console.WriteLine();
        }
    }
}