using System;
using System.Runtime.InteropServices;

namespace StandardApi.Invocation.DynamicInvoke
{
    public static class Generic
    {
        public static Drone.Invocation.Data.PE.PE_META_DATA GetPeMetaData(IntPtr pModule)
        {
            var peMetadata = new Drone.Invocation.Data.PE.PE_META_DATA();
            
            try
            {
                var e_lfanew = (uint)Marshal.ReadInt32((IntPtr)((ulong)pModule + 0x3c));
                peMetadata.Pe = (uint)Marshal.ReadInt32((IntPtr)((ulong)pModule + e_lfanew));
                
                if (peMetadata.Pe != 0x4550)
                    throw new InvalidOperationException("Invalid PE signature.");
                
                peMetadata.ImageFileHeader = (Drone.Invocation.Data.PE.IMAGE_FILE_HEADER)Marshal.PtrToStructure((IntPtr)((ulong)pModule + e_lfanew + 0x4), typeof(Drone.Invocation.Data.PE.IMAGE_FILE_HEADER));
                
                var optHeader = (IntPtr)((ulong)pModule + e_lfanew + 0x18);
                var peArch = (ushort)Marshal.ReadInt16(optHeader);
                
                if (peArch == 0x010b) // Image is x32
                {
                    peMetadata.Is32Bit = true;
                    peMetadata.OptHeader32 = (Drone.Invocation.Data.PE.IMAGE_OPTIONAL_HEADER32)Marshal.PtrToStructure(optHeader, typeof(Drone.Invocation.Data.PE.IMAGE_OPTIONAL_HEADER32));
                }
                else if (peArch == 0x020b) // Image is x64
                {
                    peMetadata.Is32Bit = false;
                    peMetadata.OptHeader64 = (Drone.Invocation.Data.PE.IMAGE_OPTIONAL_HEADER64)Marshal.PtrToStructure(optHeader, typeof(Drone.Invocation.Data.PE.IMAGE_OPTIONAL_HEADER64));
                }
                else
                {
                    throw new InvalidOperationException("Invalid magic value (PE32/PE32+).");
                }
                
                var sectionArray = new Drone.Invocation.Data.PE.IMAGE_SECTION_HEADER[peMetadata.ImageFileHeader.NumberOfSections];
                
                for (var i = 0; i < peMetadata.ImageFileHeader.NumberOfSections; i++)
                {
                    var sectionPtr = (IntPtr)((ulong)optHeader + peMetadata.ImageFileHeader.SizeOfOptionalHeader + (uint)(i * 0x28));
                    sectionArray[i] = (Drone.Invocation.Data.PE.IMAGE_SECTION_HEADER)Marshal.PtrToStructure(sectionPtr, typeof(Drone.Invocation.Data.PE.IMAGE_SECTION_HEADER));
                }
                
                peMetadata.Sections = sectionArray;
            }
            catch
            {
                throw new InvalidOperationException("Invalid module base specified.");
            }
            
            return peMetadata;
        }
        
        public static IntPtr GetNativeExportAddress(IntPtr ModuleBase, string ExportName)
        {
            Data.Native.ANSI_STRING aFunc = new Data.Native.ANSI_STRING
            {
                Length = (ushort)ExportName.Length,
                MaximumLength = (ushort)(ExportName.Length + 2),
                Buffer = Marshal.StringToCoTaskMemAnsi(ExportName)
            };

            IntPtr pAFunc = Marshal.AllocHGlobal(Marshal.SizeOf(aFunc));
            Marshal.StructureToPtr(aFunc, pAFunc, true);

            IntPtr pFuncAddr = IntPtr.Zero;
            Native.LdrGetProcedureAddress(ModuleBase, pAFunc, IntPtr.Zero, ref pFuncAddr);

            Marshal.FreeHGlobal(pAFunc);

            return pFuncAddr;
        }
        
        public static IntPtr GetNativeExportAddress(IntPtr ModuleBase, short Ordinal)
        {
            IntPtr pFuncAddr = IntPtr.Zero;
            IntPtr pOrd = (IntPtr)Ordinal;

            Native.LdrGetProcedureAddress(ModuleBase, IntPtr.Zero, pOrd, ref pFuncAddr);

            return pFuncAddr;
        }
    }
}