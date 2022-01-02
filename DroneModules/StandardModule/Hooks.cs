using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Drone.Invocation.DynamicInvoke;
using Drone.Models;

namespace StandardModule;

public partial class StandardApi
{
    private readonly string[] _functions =
    {
        "NtClose",
        "NtAllocateVirtualMemory",
        "NtAllocateVirtualMemoryEx",
        "NtCreateThread",
        "NtCreateThreadEx",
        "NtCreateUserProcess",
        "NtFreeVirtualMemory",
        "NtLoadDriver",
        "NtMapViewOfSection",
        "NtOpenProcess",
        "NtProtectVirtualMemory",
        "NtQueueApcThread",
        "NtQueueApcThreadEx",
        "NtResumeThread",
        "NtSetContextThread",
        "NtSetInformationProcess",
        "NtSuspendThread",
        "NtUnloadDriver",
        "NtWriteVirtualMemory"
    };
    
    private readonly byte[] _safeBytes = {
        0x4c, 0x8b, 0xd1, // mov r10, rcx
        0xb8              // mov eax, ??
    };
    
    private void DetectHooks(DroneTask task, CancellationToken token)
    {
        if (!Environment.Is64BitProcess)
        {
            Drone.SendError(task.TaskGuid, "Only supported on x64.");
            return;
        }

        var baseAddress = GetBaseAddress("ntdll.dll");
        if (baseAddress == IntPtr.Zero)
        {
            Drone.SendError(task.TaskGuid, "Could not find ntdll base address.");
            return;
        }

        var functionAddresses = GetFunctionAddresses(baseAddress);
        var sb = new StringBuilder();
        
        foreach (var functionAddress in functionAddresses)
        {
            var instructions = new byte[4];
            Marshal.Copy(functionAddress.Value, instructions, 0, 4);
            
            if (instructions.SequenceEqual(_safeBytes)) continue;

            sb.AppendLine($"{functionAddress.Key} is hooked.");
        }

        var result = sb.ToString();

        Drone.SendResult(task.TaskGuid, string.IsNullOrWhiteSpace(result)
            ? "No hooks detected."
            : result);
    }

    private static IntPtr GetBaseAddress(string moduleName)
    {
        var self = Process.GetCurrentProcess();

        foreach (ProcessModule module in self.Modules)
        {
            if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                return module.BaseAddress;
        }
        
        return IntPtr.Zero;
    }

    private Dictionary<string, IntPtr> GetFunctionAddresses(IntPtr baseAddress)
    {
        var results = new Dictionary<string, IntPtr>();

        foreach (var function in _functions)
        {
            var functionPointer = Generic.GetExportAddress(baseAddress, function, false);
            
            if (functionPointer != IntPtr.Zero)
            {
                results.Add(function, functionPointer);
            }
        }

        return results;
    }
}