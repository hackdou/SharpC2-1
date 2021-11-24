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
        private readonly C2Profile _default;
        
        public ProcessInjectionChecks(C2Profile.ProcessInjectionBlock block)
        {
            _block = block;
            _default = new C2Profile();
        }

        public void CheckAllocation()
        {
            string[] opts = { "NtWriteVirtualMemory", "NtMapViewOfSection" };

            if (string.IsNullOrWhiteSpace(_block.Allocation))
                Console.WriteLine($"[!!!] Allocation Technique not defined.  It will have its default value of {_default.ProcessInjection.Allocation}.");

            if (!opts.Contains(_block.Allocation))
                Console.WriteLine($"[!!!] Allocation Technique is not valid.  Options are {string.Join(", ", opts)}.");
        }

        public void CheckExecution()
        {
            string[] opts = { "NtCreateThreadEx", "RtlCreateUserThread", "CreateRemoteThread" };

            if (string.IsNullOrWhiteSpace(_block.Execution))
                Console.WriteLine($"[!!!] Execution Technique not defined.  It will have its default value of {_default.ProcessInjection.Execution}.");

            if (!opts.Contains(_block.Execution))
                Console.WriteLine($"[!!!] Execution Technique is not valid.  Options are {string.Join(", ", opts)}.");
        }
    }
}