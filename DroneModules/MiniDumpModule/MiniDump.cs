using MiniDumpModule.Decryptor;
using MiniDumpModule.Streams;

namespace MiniDumpModule;

public struct MiniDump
{
    public Header.MinidumpHeader Header;
    public SystemInfo.MINIDUMP_SYSTEM_INFO SystemInfo;
    public List<ModuleList.MinidumpModule> Modules;
    public MINIDUMP_MEMORY64.MinidumpMemory64List MemorySegments64;
    public MINIDUMP_MEMORY86.MinidumpMemory86List MemorySegments;
    public BinaryReader BinaryReader;
    public LsaDecryptor.LsaKeys LsaKeys;
    public List<Logon> LogonList;
    public List<KerberosSessions.KerberosLogonItem> Klogonlist;
}