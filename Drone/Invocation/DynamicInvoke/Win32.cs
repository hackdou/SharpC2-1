using System;
using System.Runtime.InteropServices;

namespace Drone.Invocation.DynamicInvoke
{
    public static class Win32
    {
        public static class Kernel32
        {
            public static bool PeekNamedPipe(IntPtr pipeHandle, ref uint bytesToRead)
            {
                var parameters = new object[]
                {
                    pipeHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, bytesToRead, IntPtr.Zero
                };
                
                var result = (bool)Generic.DynamicApiInvoke("kernel32.dll", "PeekNamedPipe",
                    typeof(Delegates.PeekNamedPipe), ref parameters);

                bytesToRead = (uint)parameters[4];
                return result;
            }
        }

        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool PeekNamedPipe(
                IntPtr handle,
                IntPtr buffer,
                IntPtr nBufferSize,
                IntPtr bytesRead,
                ref uint bytesAvail,
                IntPtr bytesLeftThisMessage);
        }
    }
}