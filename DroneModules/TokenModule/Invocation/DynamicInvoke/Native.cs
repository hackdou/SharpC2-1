using System.Runtime.InteropServices;

namespace TokenModule.Invocation.DynamicInvoke;

public static class Native
{
    public static IntPtr NtOpenProcess(uint pid, Data.Win32.Kernel32.ProcessAccessFlags desiredAccess)
    {
        var hProcess = IntPtr.Zero;

        var oa = new Data.Native.OBJECT_ATTRIBUTES();
        var ci = new Data.Native.CLIENT_ID { UniqueProcess = (IntPtr)pid };

        object[] parameters = { hProcess, desiredAccess, oa, ci };

        _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll", "NtOpenProcess",
            typeof(Delegates.NtOpenProcess), ref parameters);

        hProcess = (IntPtr)parameters[0];
        return hProcess;
    }

    private struct Delegates
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint NtOpenProcess(
            ref IntPtr processHandle,
            Data.Win32.Kernel32.ProcessAccessFlags desiredAccess,
            ref Data.Native.OBJECT_ATTRIBUTES objectAttributes,
            ref Data.Native.CLIENT_ID clientId);
    }
}