using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IPayloadService
{
    Task<byte[]> GeneratePayload(Handler handler, PayloadFormat format, string source = "api");
}

public enum PayloadFormat
{
    Exe,
    Dll,
    ServiceExe,
    PowerShell,
    Shellcode
}