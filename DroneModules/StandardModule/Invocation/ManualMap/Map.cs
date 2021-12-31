using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace StandardModule.Invocation.ManualMap
{
    public static class Map
    {
        public static Data.PE.PE_MANUAL_MAP MapModuleFromDiskToSection(string dllPath)
        {
            if (!File.Exists(dllPath))
                throw new InvalidOperationException("Filepath not found.");

            var objectName = new Drone.Invocation.Data.Native.UNICODE_STRING();
            Drone.Invocation.DynamicInvoke.Native.RtlInitUnicodeString(ref objectName, @"\??\" + dllPath);
            
            var pObjectName = Marshal.AllocHGlobal(Marshal.SizeOf(objectName));
            Marshal.StructureToPtr(objectName, pObjectName, true);

            var objectAttributes = new Data.Native.OBJECT_ATTRIBUTES();
            objectAttributes.Length = Marshal.SizeOf(objectAttributes);
            objectAttributes.ObjectName = pObjectName;
            objectAttributes.Attributes = 0x40;

            var ioStatusBlock = new Data.Native.IO_STATUS_BLOCK();

            var hFile = IntPtr.Zero;
            DynamicInvoke.Native.NtOpenFile(
                ref hFile,
                Data.Win32.Kernel32.FileAccessFlags.FILE_READ_DATA |
                Data.Win32.Kernel32.FileAccessFlags.FILE_EXECUTE |
                Data.Win32.Kernel32.FileAccessFlags.FILE_READ_ATTRIBUTES |
                Data.Win32.Kernel32.FileAccessFlags.SYNCHRONIZE,
                ref objectAttributes, ref ioStatusBlock,
                Data.Win32.Kernel32.FileShareFlags.FILE_SHARE_READ |
                Data.Win32.Kernel32.FileShareFlags.FILE_SHARE_DELETE,
                Data.Win32.Kernel32.FileOpenFlags.FILE_SYNCHRONOUS_IO_NONALERT |
                Data.Win32.Kernel32.FileOpenFlags.FILE_NON_DIRECTORY_FILE
            );

            var hSection = IntPtr.Zero;
            ulong maxSize = 0;
            
            _ = DynamicInvoke.Native.NtCreateSection(
                ref hSection,
                (uint)Data.Win32.WinNT.ACCESS_MASK.SECTION_ALL_ACCESS,
                IntPtr.Zero,
                ref maxSize,
                Data.Win32.WinNT.PAGE_READONLY,
                Data.Win32.WinNT.SEC_IMAGE,
                hFile
            );

            var pBaseAddress = IntPtr.Zero;
            DynamicInvoke.Native.NtMapViewOfSection(
                hSection, (IntPtr)(-1), ref pBaseAddress,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                ref maxSize, 0x2, 0x0,
                Data.Win32.WinNT.PAGE_READWRITE
            );

            var secMapObject = new Data.PE.PE_MANUAL_MAP
            {
                PEINFO = DynamicInvoke.Generic.GetPeMetaData(pBaseAddress),
                ModuleBase = pBaseAddress
            };

            DynamicInvoke.Win32.Kernel32.CloseHandle(hFile);

            return secMapObject;
        }

        public static IntPtr AllocateFileToMemory(string filePath)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException("Filepath not found.");

            var bFile = File.ReadAllBytes(filePath);
            return AllocateBytesToMemory(bFile);
        }

        public static IntPtr AllocateBytesToMemory(byte[] fileByteArray)
        {
            var pFile = Marshal.AllocHGlobal(fileByteArray.Length);
            Marshal.Copy(fileByteArray, 0, pFile, fileByteArray.Length);
            return pFile;
        }

        public static void RelocateModule(Drone.Invocation.Data.PE.PE_META_DATA peMetadata, IntPtr moduleMemoryBase)
        {
            var idd = peMetadata.Is32Bit
                ? peMetadata.OptHeader32.BaseRelocationTable
                : peMetadata.OptHeader64.BaseRelocationTable;
            
            var imageBase = peMetadata.Is32Bit
                ? (long)((ulong)moduleMemoryBase - peMetadata.OptHeader32.ImageBase)
                : (long)((ulong)moduleMemoryBase - peMetadata.OptHeader64.ImageBase);

            var pRelocTable = (IntPtr)((ulong)moduleMemoryBase + idd.VirtualAddress);
            var nextRelocTableBlock = -1;

            while (nextRelocTableBlock != 0)
            {
                var ibr = new Data.PE.IMAGE_BASE_RELOCATION();
                ibr = (Data.PE.IMAGE_BASE_RELOCATION)Marshal.PtrToStructure(pRelocTable, typeof(Data.PE.IMAGE_BASE_RELOCATION));

                var relocCount = (ibr.SizeOfBlock - Marshal.SizeOf(ibr)) / 2;
                
                for (var i = 0; i < relocCount; i++)
                {
                    var pRelocEntry = (IntPtr)((ulong)pRelocTable + (ulong)Marshal.SizeOf(ibr) + (ulong)(i * 2));
                    var relocValue = (ushort)Marshal.ReadInt16(pRelocEntry);
                    var relocType = (ushort)(relocValue >> 12);
                    var relocPatch = (ushort)(relocValue & 0xfff);

                    if (relocType == 0) continue;
                    
                    try
                    {
                        var pPatch = (IntPtr)((ulong)moduleMemoryBase + ibr.VirtualAdress + relocPatch);
                        
                        if (relocType == 0x3)
                        {
                            var originalPtr = Marshal.ReadInt32(pPatch);
                            Marshal.WriteInt32(pPatch, originalPtr + (int)imageBase);
                        }
                        else
                        {
                            var originalPtr = Marshal.ReadInt64(pPatch);
                            Marshal.WriteInt64(pPatch, originalPtr + imageBase);
                        }
                    }
                    catch
                    {
                        throw new InvalidOperationException("Memory access violation.");
                    }
                }

                pRelocTable = (IntPtr)((ulong)pRelocTable + ibr.SizeOfBlock);
                nextRelocTableBlock = Marshal.ReadInt32(pRelocTable);
            }
        }

        public static void RewriteModuleIat(Drone.Invocation.Data.PE.PE_META_DATA peMetadata, IntPtr moduleMemoryBase)
        {
            var idd = peMetadata.Is32Bit ? peMetadata.OptHeader32.ImportTable : peMetadata.OptHeader64.ImportTable;

            if (idd.VirtualAddress == 0)
                return;

            var pImportTable = (IntPtr)((ulong)moduleMemoryBase + idd.VirtualAddress);
            
            var osVersion = new Data.Native.OSVERSIONINFOEX();
            DynamicInvoke.Native.RtlGetVersion(ref osVersion);
            
            var apiSetDict = new Dictionary<string, string>();
            
            if (osVersion.MajorVersion >= 10)
                apiSetDict = Drone.Invocation.DynamicInvoke.Generic.GetApiSetMapping();

            var counter = 0;
            var iid = new Data.Win32.Kernel32.IMAGE_IMPORT_DESCRIPTOR();
            iid = (Data.Win32.Kernel32.IMAGE_IMPORT_DESCRIPTOR)Marshal.PtrToStructure(
                (IntPtr)((ulong)pImportTable + (uint)(Marshal.SizeOf(iid) * counter)),
                typeof(Data.Win32.Kernel32.IMAGE_IMPORT_DESCRIPTOR));

            while (iid.Name != 0)
            {
                var dllName = string.Empty;

                try { dllName = Marshal.PtrToStringAnsi((IntPtr)((ulong)moduleMemoryBase + iid.Name)); }
                catch { /* ignore */ }

                if (string.IsNullOrWhiteSpace(dllName))
                    throw new InvalidOperationException("Failed to read DLL name.");
                
                var lookupKey = dllName.Substring(0, dllName.Length - 6) + ".dll";

                if (osVersion.MajorVersion >= 10 && (dllName.StartsWith("api-") || dllName.StartsWith("ext-"))
                                                 && apiSetDict.ContainsKey(lookupKey)
                                                 && apiSetDict[lookupKey].Length > 0)
                {
                    dllName = apiSetDict[lookupKey];
                }

                var hModule = Drone.Invocation.DynamicInvoke.Generic.GetLoadedModuleAddress(dllName);
                if (hModule == IntPtr.Zero)
                {
                    hModule = Drone.Invocation.DynamicInvoke.Generic.LoadModuleFromDisk(dllName);
                    
                    if (hModule == IntPtr.Zero)
                        throw new FileNotFoundException(dllName + ", unable to find the specified file.");
                }

                if (peMetadata.Is32Bit)
                {
                    var oft_itd = new Data.PE.IMAGE_THUNK_DATA32();
                    
                    for (var i = 0;; i++)
                    {
                        oft_itd = (Data.PE.IMAGE_THUNK_DATA32)Marshal.PtrToStructure(
                            (IntPtr)((ulong)moduleMemoryBase + iid.OriginalFirstThunk + (uint)(i * sizeof(uint))),
                            typeof(Data.PE.IMAGE_THUNK_DATA32));
                        
                        var ft_itd = (IntPtr)((ulong)moduleMemoryBase + iid.FirstThunk + (ulong)(i * sizeof(uint)));
                        
                        if (oft_itd.AddressOfData == 0)
                            break;

                        if (oft_itd.AddressOfData < 0x80000000)
                        {
                            var pImpByName = (IntPtr)((ulong)moduleMemoryBase + oft_itd.AddressOfData + sizeof(ushort));
                            var pFunc = IntPtr.Zero;
                            pFunc = DynamicInvoke.Generic.GetNativeExportAddress(hModule,
                                Marshal.PtrToStringAnsi(pImpByName));

                            Marshal.WriteInt32(ft_itd, pFunc.ToInt32());
                        }
                        else
                        {
                            ulong fOrdinal = oft_itd.AddressOfData & 0xFFFF;
                            var pFunc = IntPtr.Zero;
                            pFunc = DynamicInvoke.Generic.GetNativeExportAddress(hModule, (short)fOrdinal);

                            Marshal.WriteInt32(ft_itd, pFunc.ToInt32());
                        }
                    }
                }
                else
                {
                    var oft_itd = new Data.PE.IMAGE_THUNK_DATA64();
                    
                    for (var i = 0;; i++)
                    {
                        oft_itd = (Data.PE.IMAGE_THUNK_DATA64)Marshal.PtrToStructure(
                            (IntPtr)((ulong)moduleMemoryBase + iid.OriginalFirstThunk + (ulong)(i * sizeof(ulong))),
                            typeof(Data.PE.IMAGE_THUNK_DATA64));
                        
                        var ft_itd = (IntPtr)((ulong)moduleMemoryBase + iid.FirstThunk + (ulong)(i * sizeof(ulong)));
                        
                        if (oft_itd.AddressOfData == 0)
                            break;

                        if (oft_itd.AddressOfData < 0x8000000000000000)
                        {
                            var pImpByName = (IntPtr)((ulong)moduleMemoryBase + oft_itd.AddressOfData + sizeof(ushort));
                            var pFunc = IntPtr.Zero;
                            pFunc = DynamicInvoke.Generic.GetNativeExportAddress(hModule,
                                Marshal.PtrToStringAnsi(pImpByName));

                            Marshal.WriteInt64(ft_itd, pFunc.ToInt64());
                        }
                        else
                        {
                            var fOrdinal = oft_itd.AddressOfData & 0xFFFF;
                            var pFunc = IntPtr.Zero;
                            pFunc = DynamicInvoke.Generic.GetNativeExportAddress(hModule, (short)fOrdinal);

                            Marshal.WriteInt64(ft_itd, pFunc.ToInt64());
                        }
                    }
                }

                counter++;
                
                iid = (Data.Win32.Kernel32.IMAGE_IMPORT_DESCRIPTOR)Marshal.PtrToStructure(
                    (IntPtr)((ulong)pImportTable + (uint)(Marshal.SizeOf(iid) * counter)),
                    typeof(Data.Win32.Kernel32.IMAGE_IMPORT_DESCRIPTOR)
                );
            }
        }

        public static void SetModuleSectionPermissions(Drone.Invocation.Data.PE.PE_META_DATA peMetadata, IntPtr moduleMemoryBase)
        {
            var BaseOfCode = peMetadata.Is32Bit
                ? (IntPtr)peMetadata.OptHeader32.BaseOfCode
                : (IntPtr)peMetadata.OptHeader64.BaseOfCode;
            
            DynamicInvoke.Native.NtProtectVirtualMemory((IntPtr)(-1), ref moduleMemoryBase, ref BaseOfCode, Data.Win32.WinNT.PAGE_READONLY);

            foreach (var ish in peMetadata.Sections)
            {
                var isRead = (ish.Characteristics & Drone.Invocation.Data.PE.DataSectionFlags.MEM_READ) != 0;
                var isWrite = (ish.Characteristics & Drone.Invocation.Data.PE.DataSectionFlags.MEM_WRITE) != 0;
                var isExecute = (ish.Characteristics & Drone.Invocation.Data.PE.DataSectionFlags.MEM_EXECUTE) != 0;
                
                uint flNewProtect = 0;
                
                if (isRead & !isWrite & !isExecute)
                {
                    flNewProtect = Data.Win32.WinNT.PAGE_READONLY;
                }
                else if (isRead & isWrite & !isExecute)
                {
                    flNewProtect = Data.Win32.WinNT.PAGE_READWRITE;
                }
                else if (isRead & isWrite & isExecute)
                {
                    flNewProtect = Data.Win32.WinNT.PAGE_EXECUTE_READWRITE;
                }
                else if (isRead & !isWrite & isExecute)
                {
                    flNewProtect = Data.Win32.WinNT.PAGE_EXECUTE_READ;
                }
                else if (!isRead & !isWrite & isExecute)
                {
                    flNewProtect = Data.Win32.WinNT.PAGE_EXECUTE;
                }
                else
                {
                    throw new InvalidOperationException("Unknown section flag, " + ish.Characteristics);
                }

                var pVirtualSectionBase = (IntPtr)((ulong)moduleMemoryBase + ish.VirtualAddress);
                var protectSize = (IntPtr)ish.VirtualSize;

                DynamicInvoke.Native.NtProtectVirtualMemory((IntPtr)(-1), ref pVirtualSectionBase, ref protectSize, flNewProtect);
            }
        }

        public static Data.PE.PE_MANUAL_MAP MapModuleToMemory(byte[] module, IntPtr pImage)
        {
            var pModule = AllocateBytesToMemory(module);
            return MapModuleToMemory(pModule, pImage);
        }

        public static Data.PE.PE_MANUAL_MAP MapModuleToMemory(IntPtr pModule, IntPtr pImage)
        {
            var peMetadata = DynamicInvoke.Generic.GetPeMetaData(pModule);
            return MapModuleToMemory(pModule, pImage, peMetadata);
        }

        public static Data.PE.PE_MANUAL_MAP MapModuleToMemory(IntPtr pModule, IntPtr pImage,
            Drone.Invocation.Data.PE.PE_META_DATA peMetadata)
        {
            if (peMetadata.Is32Bit && IntPtr.Size == 8 || !peMetadata.Is32Bit && IntPtr.Size == 4)
            {
                Marshal.FreeHGlobal(pModule);
                throw new InvalidOperationException("The module architecture does not match the process architecture.");
            }

            var sizeOfHeaders = peMetadata.Is32Bit
                ? peMetadata.OptHeader32.SizeOfHeaders
                : peMetadata.OptHeader64.SizeOfHeaders;
            
            var bytesWritten = DynamicInvoke.Native.NtWriteVirtualMemory((IntPtr)(-1), pImage, pModule, sizeOfHeaders);

            foreach (var ish in peMetadata.Sections)
            {
                var pVirtualSectionBase = (IntPtr)((ulong)pImage + ish.VirtualAddress);
                var pRawSectionBase = (IntPtr)((ulong)pModule + ish.PointerToRawData);

                bytesWritten = DynamicInvoke.Native.NtWriteVirtualMemory((IntPtr)(-1), pVirtualSectionBase,
                    pRawSectionBase, ish.SizeOfRawData);

                if (bytesWritten != ish.SizeOfRawData)
                    throw new InvalidOperationException("Failed to write to memory.");
            }

            RelocateModule(peMetadata, pImage);
            RewriteModuleIat(peMetadata, pImage);
            SetModuleSectionPermissions(peMetadata, pImage);

            Marshal.FreeHGlobal(pModule);

            return new Data.PE.PE_MANUAL_MAP
            {
                ModuleBase = pImage,
                PEINFO = peMetadata
            };
        }

        public static void FreeModule(Data.PE.PE_MANUAL_MAP map)
        {
            if (!string.IsNullOrEmpty(map.DecoyModule))
            {
                DynamicInvoke.Native.NtUnmapViewOfSection((IntPtr)(-1), map.ModuleBase);
            }
            else
            {
                var metaData = map.PEINFO;
                var size = metaData.Is32Bit
                    ? (IntPtr)metaData.OptHeader32.SizeOfImage
                    : (IntPtr)metaData.OptHeader64.SizeOfImage;
                
                var pModule = map.ModuleBase;

                DynamicInvoke.Native.NtFreeVirtualMemory((IntPtr)(-1), ref pModule, ref size,
                    Data.Win32.Kernel32.MEM_RELEASE);
            }
        }
    }
}