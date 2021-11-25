// Author: Ryan Cobb (@cobbr_io)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Drone.Invocation.DynamicInvoke
{
    public static class Generic
    {
        public static object DynamicApiInvoke(string dllName, string functionName, Type functionDelegateType,
            ref object[] parameters, bool canLoadFromDisk = false, bool resolveForwards = true)
        {
            var pFunction = GetLibraryAddress(dllName, functionName, canLoadFromDisk, resolveForwards);
            return DynamicFunctionInvoke(pFunction, functionDelegateType, ref parameters);
        }

        public static object DynamicFunctionInvoke(IntPtr functionPointer, Type functionDelegateType,
            ref object[] parameters)
        {
            var funcDelegate = Marshal.GetDelegateForFunctionPointer(functionPointer, functionDelegateType);
            return funcDelegate.DynamicInvoke(parameters);
        }

        public static IntPtr LoadModuleFromDisk(string dllPath)
        {
            var uModuleName = new Data.Native.UNICODE_STRING();
            Native.RtlInitUnicodeString(ref uModuleName, dllPath);

            var hModule = IntPtr.Zero;
            var result = Native.LdrLoadDll(IntPtr.Zero, 0, ref uModuleName, ref hModule);

            if (result != 0 || hModule == IntPtr.Zero)
                return IntPtr.Zero;

            return hModule;
        }

        public static IntPtr GetLibraryAddress(string dllName, string functionName, bool canLoadFromDisk = false,
            bool resolveForwards = true)
        {
            var hModule = GetLoadedModuleAddress(dllName);
            
            if (hModule == IntPtr.Zero && canLoadFromDisk)
            {
                hModule = LoadModuleFromDisk(dllName);
                
                if (hModule == IntPtr.Zero)
                    throw new FileNotFoundException(dllName + ", unable to find the specified file.");
            }
            else if (hModule == IntPtr.Zero)
            {
                throw new DllNotFoundException(dllName + ", Dll was not found.");
            }

            return GetExportAddress(hModule, functionName, resolveForwards);
        }

        public static IntPtr GetLoadedModuleAddress(string dllName)
        {
            var modules = Process.GetCurrentProcess().Modules;
            
            foreach (ProcessModule module in modules)
            {
                if (module.FileName.ToLower().EndsWith(dllName.ToLower()))
                    return module.BaseAddress;
            }
            
            return IntPtr.Zero;
        }

        public static IntPtr GetPebLdrModuleEntry(string dllName)
        {
            var pbi = Native.NtQueryInformationProcessBasicInformation((IntPtr)(-1));

            uint ldrDataOffset = 0;
            uint inLoadOrderModuleListOffset = 0;
            
            if (IntPtr.Size == 4)
            {
                ldrDataOffset = 0xc;
                inLoadOrderModuleListOffset = 0xC;
            }
            else
            {
                ldrDataOffset = 0x18;
                inLoadOrderModuleListOffset = 0x10;
            }

            var pebLdrData = Marshal.ReadIntPtr((IntPtr)((ulong)pbi.PebBaseAddress + ldrDataOffset));
            var pInLoadOrderModuleList = (IntPtr)((ulong)pebLdrData + inLoadOrderModuleListOffset);
            var le = (Data.Native.LIST_ENTRY)Marshal.PtrToStructure(pInLoadOrderModuleList, typeof(Data.Native.LIST_ENTRY));

            var flink = le.Flink;
            var hModule = IntPtr.Zero;
            var dte = (Data.PE.LDR_DATA_TABLE_ENTRY)Marshal.PtrToStructure(flink, typeof(Data.PE.LDR_DATA_TABLE_ENTRY));

            while (dte.InLoadOrderLinks.Flink != le.Blink)
            {
                if (Marshal.PtrToStringUni(dte.FullDllName.Buffer).EndsWith(dllName, StringComparison.OrdinalIgnoreCase))
                {
                    hModule = dte.DllBase;
                }
            
                flink = dte.InLoadOrderLinks.Flink;
                dte = (Data.PE.LDR_DATA_TABLE_ENTRY)Marshal.PtrToStructure(flink, typeof(Data.PE.LDR_DATA_TABLE_ENTRY));
            }

            return hModule;
        }

        public static IntPtr GetExportAddress(IntPtr moduleBase, string exportName, bool resolveForwards = true)
        {
            var functionPtr = IntPtr.Zero;
            
            try
            {
                var peHeader = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + 0x3C));
                var optHeader = moduleBase.ToInt64() + peHeader + 0x18;
                var magic = Marshal.ReadInt16((IntPtr)optHeader);
                long pExport = 0;
                
                if (magic == 0x010b) pExport = optHeader + 0x60;
                else pExport = optHeader + 0x70;

                var exportRva = Marshal.ReadInt32((IntPtr)pExport);
                var ordinalBase = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + exportRva + 0x10));
                var numberOfNames = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + exportRva + 0x18));
                var functionsRva = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + exportRva + 0x1C));
                var namesRva = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + exportRva + 0x20));
                var ordinalsRva = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + exportRva + 0x24));

                for (var i = 0; i < numberOfNames; i++)
                {
                    var functionName = Marshal.PtrToStringAnsi((IntPtr)(moduleBase.ToInt64() + Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + namesRva + i * 4))));

                    if (string.IsNullOrWhiteSpace(functionName)) continue;
                    if (!functionName.Equals(exportName, StringComparison.OrdinalIgnoreCase)) continue;
                    
                    var functionOrdinal = Marshal.ReadInt16((IntPtr)(moduleBase.ToInt64() + ordinalsRva + i * 2)) + ordinalBase;
                    var functionRva = Marshal.ReadInt32((IntPtr)(moduleBase.ToInt64() + functionsRva + (4 * (functionOrdinal - ordinalBase))));
                    functionPtr = (IntPtr)((long)moduleBase + functionRva);
                        
                    if (resolveForwards)
                        functionPtr = GetForwardAddress(functionPtr);

                    break;
                }
            }
            catch
            {
                throw new InvalidOperationException("Failed to parse module exports.");
            }

            if (functionPtr == IntPtr.Zero)
                throw new MissingMethodException(exportName + ", export not found.");
            
            return functionPtr;
        }

        public static IntPtr GetForwardAddress(IntPtr exportAddress, bool canLoadFromDisk = false)
        {
            var functionPtr = exportAddress;
            
            try
            {
                var forwardNames = Marshal.PtrToStringAnsi(functionPtr);
                if (string.IsNullOrWhiteSpace(forwardNames)) return functionPtr;
                
                var values = forwardNames.Split('.');

                if (values.Length > 1)
                {
                    var forwardModuleName = values[0];
                    var forwardExportName = values[1];

                    var apiSet = GetApiSetMapping();
                    var lookupKey = forwardModuleName.Substring(0, forwardModuleName.Length - 2) + ".dll";

                    if (apiSet.ContainsKey(lookupKey))
                        forwardModuleName = apiSet[lookupKey];
                    else
                        forwardModuleName += ".dll";

                    var hModule = GetPebLdrModuleEntry(forwardModuleName);

                    if (hModule == IntPtr.Zero && canLoadFromDisk == true)
                        hModule = LoadModuleFromDisk(forwardModuleName);

                    if (hModule != IntPtr.Zero)
                        functionPtr = GetExportAddress(hModule, forwardExportName);
                }
            }
            catch
            {
                // Do nothing, it was not a forward
            }
            
            return functionPtr;
        }

        public static Dictionary<string, string> GetApiSetMapping()
        {
            var pbi = Native.NtQueryInformationProcessBasicInformation((IntPtr)(-1));
            var apiSetMapOffset = IntPtr.Size == 4 ? (uint)0x38 : 0x68;
            var apiSetDict = new Dictionary<string, string>();
            var pApiSetNamespace = Marshal.ReadIntPtr((IntPtr)((ulong)pbi.PebBaseAddress + apiSetMapOffset));
            var setNamespace = (Data.PE.ApiSetNamespace)Marshal.PtrToStructure(pApiSetNamespace, typeof(Data.PE.ApiSetNamespace));
            
            for (var i = 0; i < setNamespace.Count; i++)
            {
                var setEntry = new Data.PE.ApiSetNamespaceEntry();
                var pSetEntry = (IntPtr)((ulong)pApiSetNamespace + (ulong)setNamespace.EntryOffset + (ulong)(i * Marshal.SizeOf(setEntry)));
                setEntry = (Data.PE.ApiSetNamespaceEntry)Marshal.PtrToStructure(pSetEntry, typeof(Data.PE.ApiSetNamespaceEntry));

                var apiSetEntryName = Marshal.PtrToStringUni((IntPtr)((ulong)pApiSetNamespace + (ulong)setEntry.NameOffset), setEntry.NameLength/2);
                var apiSetEntryKey = apiSetEntryName.Substring(0, apiSetEntryName.Length - 2) + ".dll";
                var setValue = new Data.PE.ApiSetValueEntry();
                var pSetValue = IntPtr.Zero;

                switch (setEntry.ValueLength)
                {
                    case 1:
                        pSetValue = (IntPtr)((ulong)pApiSetNamespace + (ulong)setEntry.ValueOffset);
                        break;
                    
                    case > 1:
                    {
                        for (var j = 0; j < setEntry.ValueLength; j++)
                        {
                            var host = (IntPtr)((ulong)pApiSetNamespace + (ulong)setEntry.ValueOffset + (ulong)Marshal.SizeOf(setValue) * (ulong)j);
                            
                            if (Marshal.PtrToStringUni(host) != apiSetEntryName)
                                pSetValue = (IntPtr)((ulong)pApiSetNamespace + (ulong)setEntry.ValueOffset + (ulong)Marshal.SizeOf(setValue) * (ulong)j);
                        }

                        if (pSetValue == IntPtr.Zero)
                            pSetValue = (IntPtr)((ulong)pApiSetNamespace + (ulong)setEntry.ValueOffset);
                        
                        break;
                    }
                }

                setValue = (Data.PE.ApiSetValueEntry)Marshal.PtrToStructure(pSetValue, typeof(Data.PE.ApiSetValueEntry));
                
                var apiSetValue = string.Empty;
                
                if (setValue.ValueCount != 0)
                {
                    var pValue = (IntPtr)((ulong)pApiSetNamespace + (ulong)setValue.ValueOffset);
                    apiSetValue = Marshal.PtrToStringUni(pValue, setValue.ValueCount/2);
                }

                apiSetDict.Add(apiSetEntryKey, apiSetValue);
            }

            return apiSetDict;
        }

        public static void CallMappedDllModule(Data.PE.PE_META_DATA peMetadata, IntPtr moduleMemoryBase)
        {
            var lpEntryPoint = peMetadata.Is32Bit
                ? (IntPtr)((ulong)moduleMemoryBase + peMetadata.OptHeader32.AddressOfEntryPoint)
                : (IntPtr)((ulong)moduleMemoryBase + peMetadata.OptHeader64.AddressOfEntryPoint);

            if (lpEntryPoint == moduleMemoryBase) return;
            
            var fDllMain = (Data.PE.DllMain)Marshal.GetDelegateForFunctionPointer(lpEntryPoint, typeof(Data.PE.DllMain));

            try
            {
                if (!fDllMain(moduleMemoryBase, Data.PE.DLL_PROCESS_ATTACH, IntPtr.Zero))
                    throw new InvalidOperationException("Call to entry point failed -> DLL_PROCESS_ATTACH");
            }
            catch
            {
                throw new InvalidOperationException("Invalid entry point -> DLL_PROCESS_ATTACH");
            }
        }

        public static object CallMappedDllModuleExport(Data.PE.PE_META_DATA peMetadata, IntPtr moduleMemoryBase,
            string exportName, Type functionDelegateType, object[] parameters, bool callEntry = true)
        {
            if (callEntry)
                CallMappedDllModule(peMetadata, moduleMemoryBase);

            var pFunc = GetExportAddress(moduleMemoryBase, exportName);
            return DynamicFunctionInvoke(pFunc, functionDelegateType, ref parameters);
        }
    }
}