using System.IO;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class ExePayload : Payload
    {
        public override async Task Generate()
        {
            var drone = await GetDroneModuleDef();
            drone.ConvertModuleToExe();

            await using var ms = new MemoryStream();
            drone.Write(ms);
            Bytes = ms.ToArray();
        }
    }
}