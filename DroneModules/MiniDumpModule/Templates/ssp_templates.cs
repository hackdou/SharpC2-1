using System;
using System.Runtime.InteropServices;
using MiniDumpModule.Invocation.Data;
using MiniDumpModule.Streams;
using static MiniDumpModule.Helpers;
using Native = Drone.Invocation.Data.Native;

namespace MiniDumpModule.Templates
{
    public class ssp
    {
        public struct SspTemplate
        {
            public byte[] signature;
            public int first_entry_offset;
            public KIWI_SSP_CREDENTIAL_LIST_ENTRY list_entry;
        }

        public static SspTemplate get_template(SystemInfo.MINIDUMP_SYSTEM_INFO sysinfo)
        {
            var template = new SspTemplate();
            template.list_entry = new KIWI_SSP_CREDENTIAL_LIST_ENTRY();

            if (sysinfo.ProcessorArchitecture == SystemInfo.PROCESSOR_ARCHITECTURE.AMD64)
            {
                if (sysinfo.BuildNumber < (int)SystemInfo.WindowsMinBuild.WIN_VISTA)
                {
                    template.signature = new byte[] { 0xc7, 0x43, 0x24, 0x43, 0x72, 0x64, 0x41, 0xff, 0x15 };
                    template.first_entry_offset = 16;
                }
                else if ((int)SystemInfo.WindowsMinBuild.WIN_VISTA <= sysinfo.BuildNumber && sysinfo.BuildNumber < (int)SystemInfo.WindowsBuild.WIN_10_1507)
                {
                    template.signature = new byte[]
                        {0xc7, 0x47, 0x24, 0x43, 0x72, 0x64, 0x41, 0x48, 0x89, 0x47, 0x78, 0xff, 0x15};
                    template.first_entry_offset = 20;
                }
                else if (sysinfo.BuildNumber >= (int)SystemInfo.WindowsBuild.WIN_10_1507)
                {
                    template.signature = new byte[] { 0x24, 0x43, 0x72, 0x64, 0x41, 0xff, 0x15 };
                    template.first_entry_offset = 14;
                }
                else
                {
                    //currently doesnt make sense, but keeping it here for future use
                    throw new Exception($"Unknown buildnumber! {sysinfo.BuildNumber}");
                }
            }
            else if (sysinfo.ProcessorArchitecture == SystemInfo.PROCESSOR_ARCHITECTURE.INTEL)
            {
                template.signature = new byte[] { 0x1c, 0x43, 0x72, 0x64, 0x41, 0xff, 0x15 };
                template.first_entry_offset = 12;
            }
            else
            {
                throw new Exception($"Unknown architecture! {sysinfo.ProcessorArchitecture}");
            }

            return template;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KIWI_GENERIC_PRIMARY_CREDENTIAL
        {
            public Native.UNICODE_STRING Domain;
            public Native.UNICODE_STRING UserName;
            public Native.UNICODE_STRING Password;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KIWI_SSP_CREDENTIAL_LIST_ENTRY
        {
            public long Flink;
            public long Blink;
            public uint References;
            public uint CredentialReferences;
            public Win32.WinNT.LUID LogonId;
            public uint unk0;
            public uint unk1;
            public uint unk2;
            public KIWI_GENERIC_PRIMARY_CREDENTIAL credentials;
        };
    }
}