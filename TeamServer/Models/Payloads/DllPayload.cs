using System.IO;
using System.Threading.Tasks;

using dnlib.DotNet.Writer;
using dnlib.PE;

namespace TeamServer.Models
{
    public class DllPayload : Payload
    {
        public string DllExport { get; set; }
        
        public override async Task Generate()
        {
            var drone = await GetDroneModuleDef();
            drone.AddUnmanagedExport(DllExport);

            var opts = new ModuleWriterOptions(drone)
            {
                PEHeadersOptions = { Machine = Machine.AMD64 },
                Cor20HeaderOptions = { Flags = 0 }
            };
            
            await using var ms = new MemoryStream();
            drone.Write(ms, opts);
            Bytes = ms.ToArray();
        }
    }
}