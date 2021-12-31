using System.Runtime.InteropServices;

using Drone.Invocation.DynamicInvoke;

namespace MiniDumpModule.Invocation.DynamicInvoke;

public static class Win32
{
    public static bool MiniDumpWriteDump(IntPtr hProcess, int pid, IntPtr hFile)
    {
        object[] parameters =
        {
            hProcess, (uint)pid, hFile, (uint)2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero
        };
        
        return (bool) Generic.DynamicApiInvoke("dbgcore.dll", "MiniDumpWriteDump",
            typeof (Delegates.MiniDumpWriteDump), ref parameters, true);
    }

    public static bool FreeLibrary(IntPtr hModule)
    {
        object[] parameters = { hModule };
        
        return (bool) Generic.DynamicApiInvoke("kernel32.dll", "FreeLibrary",
            typeof (Delegates.FreeLibrary), ref parameters);
    } 

    public static bool CloseHandle(IntPtr handle)
    {
        var parameters = new object[]{ handle };
        
        return (bool) Generic.DynamicApiInvoke("kernel32.dll", "CloseHandle",
            typeof (Delegates.CloseHandle), ref parameters);
    }

    public static bool ConvertSidToStringSid(byte[] pSID, out IntPtr ptrSid)
    {
        var ptr = IntPtr.Zero;
        var parameters = new object[]{ pSID, ptr };
        
        var result = (bool) Generic.DynamicApiInvoke("advapi32.dll", "ConvertSidToStringSidW",
            typeof (Delegates.ConvertSidToStringSid), ref parameters);

        ptrSid = ptr;
        return result;
    }
    
    public struct Delegates
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate bool MiniDumpWriteDump(
            IntPtr hProcess,
            uint ProcessId,
            IntPtr hFile,
            uint DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallbackParam);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool FreeLibrary(IntPtr hModule);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool CloseHandle(IntPtr hProcess);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetFileSize(
            IntPtr hFile,
            out uint lpFileSizeHigh);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint SetFilePointer(
            IntPtr hFile,
            uint liDistanceToMove,
            IntPtr lpNewFilePointer,
            MOVE_METHOD dwMoveMethod);
            
        public enum MOVE_METHOD : uint
        {
            FILE_BEGIN = 0,
            FILE_CURRENT = 1,
            FILE_END = 2
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool WriteFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ConvertSidToStringSid(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pSID,
            out IntPtr ptrSid);
    }
}