using System;
using System.IO;
using System.Linq;
using System.Reflection;

using TeamServer.Models;

namespace C2Lint
{
    public class ProcessInjectionChecks
    {
        private readonly C2Profile.ProcessInjectionBlock _block;
        private readonly Type[] _types;
        
        public ProcessInjectionChecks(C2Profile.ProcessInjectionBlock block)
        {
            _block = block;
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "drone.dll");
            var bytes = File.ReadAllBytes(path);
            var asm = Assembly.Load(bytes);
            
            _types = asm.GetTypes();
        }

        public void CheckAllocation()
        {
            var baseType = _types.Single(t => t.FullName == "Drone.DInvoke.Injection.AllocationTechnique");
            var allocTypes = _types.Where(t => t.BaseType == baseType).ToArray();

            var found = allocTypes.Any(t =>
                t.Name.Equals(_block.Allocation, StringComparison.OrdinalIgnoreCase));

            if (found) return;
            
            Console.WriteLine($"[!!!] Allocation Technique is not valid.  Options are {string.Join(", ", allocTypes.Select(t => t.Name))}.");
            Console.WriteLine();
        }

        public void CheckExecution()
        {
            var baseType = _types.Single(t => t.FullName == "Drone.DInvoke.Injection.ExecutionTechnique");
            var execTypes = _types.Where(t => t.BaseType == baseType).ToArray();

            var found = execTypes.Any(t =>
                t.Name.Equals(_block.Execution, StringComparison.OrdinalIgnoreCase));

            if (found) return;
            
            Console.WriteLine($"[!!!] Execution Technique is not valid.  Options are {string.Join(", ", execTypes.Select(t => t.Name))}.");
            Console.WriteLine();
        }
    }
}