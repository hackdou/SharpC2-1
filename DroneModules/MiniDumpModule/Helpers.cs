using MiniDumpModule.Streams;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using MiniDumpModule.Invocation.DynamicInvoke;
using Native = Drone.Invocation.Data.Native;

namespace MiniDumpModule;

public class Helpers
{
    public const int LM_NTLM_HASH_LENGTH = 16;
    public const int SHA_DIGEST_LENGTH = 20;

    public static DateTime ToDateTime(FILETIME time)
    {
        var fileTime = ((long)time.dwHighDateTime << 32) | (uint)time.dwLowDateTime;

        try
        {
            return DateTime.FromFileTime(fileTime);
        }
        catch
        {
            return DateTime.FromFileTime(0xFFFFFFFF);
        }
    }

    public static List<long> find_all_global(BinaryReader fileBinaryReader, byte[] pattern,
        byte[] allocationprotect = null)
    {
        var list = new List<long>();
        if (allocationprotect == null)
            allocationprotect = new byte[] { 0x04 };

        fileBinaryReader.BaseStream.Seek(0, 0);
        var data = fileBinaryReader.ReadBytes((int)fileBinaryReader.BaseStream.Length);
        list = AllPatternAt(data, pattern);
        return list;
    }

    //https://github.com/skelsec/pypykatz/blob/bd1054d1aa948133a697a1dfcb57a5c6463be41a/pypykatz/lsadecryptor/package_commons.py#L64
    public static long find_signature(MiniDump minidump, string module_name, byte[] signature)
    {
        return find_in_module(minidump, module_name, signature);
    }

    //https://github.com/skelsec/minidump/blob/96d6b64dba679df14f5f78c64c3a045be8c4f1f1/minidump/minidumpreader.py#L268
    public static long find_in_module(MiniDump minidump, string module_name, byte[] pattern,
        bool find_first = false, bool reverse = false)
    {
        return search_module(minidump, module_name, pattern, find_first = find_first, reverse = reverse);
    }

    //https://github.com/skelsec/minidump/blob/96d6b64dba679df14f5f78c64c3a045be8c4f1f1/minidump/minidumpreader.py#L323
    public static long search_module(MiniDump minidump, string module_name, byte[] pattern,
        bool find_first = false, bool reverse = false, int chunksize = 10 * 1024)
    {
        var pos = minidump.BinaryReader.BaseStream.Position;
        var mod = get_module_by_name(module_name, minidump.Modules);
        var memory_segments = new List<MinidumpMemory.MinidumpMemorySegment>();
        bool is_fulldump;
        if (minidump.SystemInfo.ProcessorArchitecture == SystemInfo.PROCESSOR_ARCHITECTURE.AMD64)
        {
            memory_segments = minidump.MemorySegments64.memory_segments;
            is_fulldump = true;
        }
        else
        {
            memory_segments = minidump.MemorySegments.memory_segments;
            is_fulldump = false;
        }

        var needles = new byte[] { };
        foreach (var ms in memory_segments)
        {
            if (mod.baseaddress <= ms.start_virtual_address && ms.start_virtual_address <= mod.endaddress)
            {
                minidump.BinaryReader.BaseStream.Seek(ms.start_file_address, 0);
                var data = minidump.BinaryReader.ReadBytes((int)ms.size);
                minidump.BinaryReader.BaseStream.Seek(pos, 0);
                var offset = PatternAt(data, pattern);
                if (offset is not -1)
                {
                    return ms.start_file_address + offset;
                }
            }
        }

        return 0;
    }

    public static long Rva2offset(MiniDump minidump, long virutal_address)
    {
        var memory_segments = new List<MinidumpMemory.MinidumpMemorySegment>();
        bool is_fulldump;
        if (minidump.SystemInfo.ProcessorArchitecture == SystemInfo.PROCESSOR_ARCHITECTURE.AMD64)
        {
            memory_segments = minidump.MemorySegments64.memory_segments;
            is_fulldump = true;
        }
        else
        {
            memory_segments = minidump.MemorySegments.memory_segments;
            is_fulldump = false;
        }

        foreach (var ms in memory_segments)
        {
            if (ms.start_virtual_address <= virutal_address && ms.end_virtual_address >= virutal_address)
            {
                if (ms.start_virtual_address < virutal_address)
                {
                    var offset = (int)(virutal_address - ms.start_virtual_address);
                    return ms.start_file_address + offset;
                }

                return ms.start_file_address;
            }
        }

        return 0;
    }

    public static string ByteArrayToString(byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 2);
        
        foreach (var b in ba)
            hex.AppendFormat("0x{0:x2} ", b);
        
        return hex.ToString();
    }

    public static int PatternAt(byte[] src, byte[] pattern)
    {
        var maxFirstCharSlot = src.Length - pattern.Length + 1;
        for (var i = 0; i < maxFirstCharSlot; i++)
        {
            if (src[i] != pattern[0]) // compare only first byte
                continue;

            // found a match on first byte, now try to match rest of the pattern
            for (var j = pattern.Length - 1; j >= 1; j--)
            {
                if (src[i + j] != pattern[j]) break;
                if (j == 1) return i;
            }
        }

        return -1;
    }

    public static List<long> AllPatternAt(byte[] src, byte[] pattern)
    {
        var list = new List<long>();
        var maxFirstCharSlot = src.Length - pattern.Length + 1;
        for (var i = 0; i < maxFirstCharSlot; i++)
        {
            if (src[i] != pattern[0]) // compare only first byte
                continue;

            // found a match on first byte, now try to match rest of the pattern
            for (var j = pattern.Length - 1; j >= 1; j--)
            {
                if (src[i + j] != pattern[j]) break;
                if (j == 1) list.Add(i);
            }
        }

        return list;
    }

    //https://github.com/skelsec/minidump/blob/96d6b64dba679df14f5f78c64c3a045be8c4f1f1/minidump/minidumpreader.py#L311
    public static ModuleList.MinidumpModule get_module_by_name(string module_name,
        List<ModuleList.MinidumpModule> modules)
    {
        return modules.FirstOrDefault(item => item.name.Contains(module_name));
    }

    //https://github.com/skelsec/pypykatz/blob/bd1054d1aa948133a697a1dfcb57a5c6463be41a/pypykatz/commons/common.py#L168
    public static ulong get_ptr_with_offset(BinaryReader fileBinaryReader, long pos,
        SystemInfo.MINIDUMP_SYSTEM_INFO sysinfo)
    {
        if (sysinfo.ProcessorArchitecture == SystemInfo.PROCESSOR_ARCHITECTURE.AMD64)
        {
            fileBinaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
            var ptr = ReadUInt32(fileBinaryReader);
            return (ulong)(pos + 4 + ptr);
        }
        else
        {
            fileBinaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
            var ptr = ReadUInt16(fileBinaryReader);
            return ptr;
        }
    }

    //https://github.com/skelsec/pypykatz/blob/bd1054d1aa948133a697a1dfcb57a5c6463be41a/pypykatz/commons/common.py#L162
    public static ulong get_ptr(BinaryReader fileBinaryReader, long pos, SystemInfo.MINIDUMP_SYSTEM_INFO sysinfo)
    {
        fileBinaryReader.BaseStream.Seek(pos, 0);
        if (sysinfo.ProcessorArchitecture == SystemInfo.PROCESSOR_ARCHITECTURE.AMD64)
        {
            var ptr = ReadUInt32(fileBinaryReader);
            return ptr;
        }
        else
        {
            var ptr = ReadUInt16(fileBinaryReader);
            return ptr;
        }
    }

    public static T ReadStruct<T>(byte[] array) where T : struct
    {
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        var mystruct = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return mystruct;
    }

    public static string ExtractSid(MiniDump minidump, long pSid)
    {
        byte nbAuth;
        int sizeSid;

        var pSidInt = ReadInt64(minidump.BinaryReader, pSid);
        minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, pSidInt) + 8, 0);
        var nbAuth_b = minidump.BinaryReader.ReadBytes(1);
        nbAuth = nbAuth_b[0];
        sizeSid = 4 * nbAuth + 6 + 1 + 1;

        minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, pSidInt), 0);
        var sid_b = minidump.BinaryReader.ReadBytes(sizeSid);

        Win32.ConvertSidToStringSid(sid_b, out var ptrSid);

        return Marshal.PtrToStringAuto(ptrSid);
    }

    public static Native.UNICODE_STRING ExtractUnicodeString(BinaryReader fileStreamReader)
    {
        Native.UNICODE_STRING str;

        var strBytes = fileStreamReader.ReadBytes(Marshal.SizeOf(typeof(Native.UNICODE_STRING)));
        str = ReadStruct<Native.UNICODE_STRING>(strBytes);

        return str;
    }

    public static string ExtractUnicodeStringString(MiniDump minidump, Native.UNICODE_STRING str)
    {
        if (str.MaximumLength == 0) return null;

        minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, (long)str.Buffer), 0);
        var resultBytes = minidump.BinaryReader.ReadBytes(str.MaximumLength);

        var encoder = new UnicodeEncoding(false, false, true);
        try
        {
            return encoder.GetString(resultBytes);
        }
        catch (Exception)
        {
            return PrintHexBytes(resultBytes);
        }
    }

    public static string PrintHexBytes(byte[] byteArray)
    {
        var res = new StringBuilder(byteArray.Length * 3);
        for (var i = 0; i < byteArray.Length; i++)
            res.AppendFormat(NumberFormatInfo.InvariantInfo, "{0:x2} ", byteArray[i]);
        return res.ToString();
    }

    public static int FieldOffset<T>(string fieldName)
    {
        return Marshal.OffsetOf(typeof(T), fieldName).ToInt32();
    }

    public static int StructFieldOffset(Type s, string field)
    {
        var ex = typeof(Helpers);
        var mi = ex.GetMethod("FieldOffset");
        var miConstructed = mi.MakeGenericMethod(s);
        object[] args = { field };
        return (int)miConstructed.Invoke(null, args);
    }

    public static Native.UNICODE_STRING ExtractUnicodeString(BinaryReader fileStreamReader, long offset)
    {
        Native.UNICODE_STRING str;
        fileStreamReader.BaseStream.Seek(offset, 0);
        var strBytes = fileStreamReader.ReadBytes(Marshal.SizeOf(typeof(Native.UNICODE_STRING)));
        str = ReadStruct<Native.UNICODE_STRING>(strBytes);

        return str;
    }

    public static byte[] GetBytes(byte[] source, long startindex, int lenght)
    {
        var resBytes = new byte[lenght];
        Array.Copy(source, startindex, resBytes, 0, resBytes.Length);
        return resBytes;
    }

    public static string PrintHashBytes(byte[] byteArray)
    {
        if (byteArray == null)
            return string.Empty;

        var res = new StringBuilder(byteArray.Length * 2);
        for (var i = 0; i < byteArray.Length; i++)
            res.AppendFormat(NumberFormatInfo.InvariantInfo, "{0:x2}", byteArray[i]);
        return res.ToString();
    }

    public static string ExtractANSIStringString(MiniDump minidump, Native.UNICODE_STRING str)
    {
        if (str.MaximumLength == 0) return null;

        minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, (long)str.Buffer), 0);
        var resultBytes = minidump.BinaryReader.ReadBytes(str.MaximumLength);
        var pinnedArray = GCHandle.Alloc(resultBytes, GCHandleType.Pinned);
        var tmp_p = pinnedArray.AddrOfPinnedObject();
        var result = Marshal.PtrToStringAnsi(tmp_p);
        pinnedArray.Free();

        return result;
    }

    public static string get_from_rva(int rva, BinaryReader fileBinaryReader)
    {
        var pos = fileBinaryReader.BaseStream.Position;
        fileBinaryReader.BaseStream.Seek(rva, 0);
        var length = ReadUInt32(fileBinaryReader);
        var data = fileBinaryReader.ReadBytes((int)length);
        ////Array.Reverse(data);
        fileBinaryReader.BaseStream.Seek(pos, 0);
        var name = Encoding.Unicode.GetString(data);
        return name;
    }

    public static string PrintProperties(object myObj, string header = "", int offset = 0)
    {
        var sb = new StringBuilder();
        
        var trail = string.Concat(Enumerable.Repeat(" ", offset));

        if (!string.IsNullOrWhiteSpace(header))
            sb.AppendLine(header);

        foreach (var prop in myObj.GetType().GetProperties())
        {
            try
            {
                if (!string.IsNullOrWhiteSpace((string)prop.GetValue(myObj, null)))
                    sb.AppendLine(trail + prop.Name + ": " + prop.GetValue(myObj, null));
            }
            catch
            {
                sb.AppendLine(trail + prop.Name + ": " + prop.GetValue(myObj, null));
            }
        }

        foreach (var field in myObj.GetType().GetFields())
        {
            try
            {
                if (!string.IsNullOrWhiteSpace((string)field.GetValue(myObj)))
                    sb.AppendLine(trail + field.Name + ": " + field.GetValue(myObj));
            }
            catch
            {
                sb.AppendLine(trail + field.Name + ": " + field.GetValue(myObj));
            }
        }

        return sb.ToString();
    }

    public static string ReadString(BinaryReader fileBinaryReader, int Length)
    {
        var data = fileBinaryReader.ReadBytes(Length);
        Array.Reverse(data);
        return Encoding.Unicode.GetString(data);
    }

    public static short ReadInt16(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(2);
        //Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }

    public static int ReadInt32(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(4);
        //Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public static long ReadInt64(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(8);
        //Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }

    public static uint ReadInt8(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(1)[0];
        //Array.Reverse(data);
        return data;
    }

    public static ushort ReadUInt16(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(2);
        //Array.Reverse(data);
        return BitConverter.ToUInt16(data, 0);
    }

    public static uint ReadUInt32(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(4);
        //Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

    public static ulong ReadUInt64(BinaryReader fileBinaryReader)
    {
        var data = fileBinaryReader.ReadBytes(8);
        //Array.Reverse(data);
        return BitConverter.ToUInt64(data, 0);
    }

    public static short ReadInt16(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(2);
        //Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }

    public static int ReadInt32(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(4);
        //Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public static long ReadInt64(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(8);
        //Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }

    public static uint ReadInt8(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(1)[0];
        //Array.Reverse(data);
        return data;
    }

    public static ushort ReadUInt16(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(2);
        //Array.Reverse(data);
        return BitConverter.ToUInt16(data, 0);
    }

    public static uint ReadUInt32(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(4);
        //Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

    public static ulong ReadUInt64(BinaryReader fileBinaryReader, long offset)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(8);
        //Array.Reverse(data);
        return BitConverter.ToUInt64(data, 0);
    }

    public static byte[] ReadBytes(BinaryReader fileBinaryReader, long offset, int length)
    {
        fileBinaryReader.BaseStream.Seek(offset, 0);
        var data = fileBinaryReader.ReadBytes(length);
        //Array.Reverse(data);
        return data;
    }

    public static string FormatOutput(MiniDump miniDump)
    {
        var sb = new StringBuilder();

        foreach (var logon in miniDump.LogonList)
        {
            if (logon.Wdigest is not null || logon.Msv is not null || logon.Kerberos is not null || logon.Tspkg is not null ||
                logon.Credman is not null || logon.Ssp is not null || logon.LiveSsp is not null || logon.Dpapi is not null ||
                logon.Cloudap is not null)
            {
                sb.AppendLine("=====================================================================");
                //Helpers.PrintProperties(log);
                sb.AppendLine($"[*] LogonId:     {logon.LogonId.HighPart}:{logon.LogonId.LowPart}");
                if (!string.IsNullOrWhiteSpace(logon.LogonType))
                    sb.AppendLine($"[*] LogonType:   {logon.LogonType}");
                sb.AppendLine($"[*] Session:     {logon.Session}");
                if (logon.LogonTime.dwHighDateTime is not 0)
                    sb.AppendLine($"[*] LogonTime:   {ToDateTime(logon.LogonTime):yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"[*] UserName:    {logon.UserName}");
                if (!string.IsNullOrWhiteSpace(logon.SID))
                    sb.AppendLine($"[*] SID:         {logon.SID}");
                if (!string.IsNullOrWhiteSpace(logon.LogonDomain))
                    sb.AppendLine($"[*] LogonDomain: {logon.LogonDomain}");
                if (!string.IsNullOrWhiteSpace(logon.LogonServer))
                    sb.AppendLine($"[*] LogonServer: {logon.LogonServer}");
            }

            if (logon.Msv is not null)
            {
                sb.AppendLine(PrintProperties(logon.Msv, "[*] Msv", 4));
            }

            if (logon.Kerberos is not null)
            {
                sb.AppendLine(PrintProperties(logon.Kerberos, "[*] Kerberos", 4));
            }

            if (logon.Wdigest is not null)
            {
                foreach (var wd in logon.Wdigest)
                    sb.AppendLine(PrintProperties(wd, "[*] Wdigest", 4));
            }

            if (logon.Ssp is not null)
            {
                foreach (var s in logon.Ssp)
                    sb.AppendLine(PrintProperties(s, "[*] Ssp", 4));
            }

            if (logon.Tspkg is not null)
            {
                foreach (var ts in logon.Tspkg)
                    sb.AppendLine(PrintProperties(ts, "[*] TsPkg", 4));
            }

            if (logon.Credman is not null)
            {
                foreach (var cm in logon.Credman)
                    sb.AppendLine(PrintProperties(cm, "[*] CredMan", 4));
            }

            if (logon.Dpapi is not null)
            {
                foreach (var dpapi in logon.Dpapi)
                    sb.AppendLine(PrintProperties(dpapi, "[*] Dpapi", 4));
            }

            if (logon.Cloudap is not null)
            {
                foreach (var cap in logon.Cloudap)
                    sb.AppendLine(PrintProperties(cap, "[*] CloudAp", 4));
            }
        }

        return sb.ToString();
    }
}