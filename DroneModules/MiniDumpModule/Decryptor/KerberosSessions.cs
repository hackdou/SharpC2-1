using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MiniDumpModule.Templates;
using static MiniDumpModule.Helpers;

namespace MiniDumpModule.Decryptor
{
    public class KerberosSessions
    {
        public static List<KerberosLogonItem> FindSessions(MiniDump minidump, kerberos.KerberosTemplate template)
        {
            var klogonlist = new List<KerberosLogonItem>();

            long position = find_signature(minidump, "kerberos.dll", template.signature);
            if (position == 0)
            {
                Console.WriteLine("[x] Error: Could not find KerberosSessionList signature\n");
                return klogonlist;
            }
            var ptr_entry_loc = get_ptr_with_offset(minidump.BinaryReader, (position + template.first_entry_offset), minidump.SystemInfo);
            var ptr_entry = global::MiniDumpModule.Helpers.ReadUInt64(minidump.BinaryReader, (long)ptr_entry_loc);
            //long kerbUnloadLogonSessionTableAddr = Rva2offset(minidump, (long)ptr_entry);
            //minidump.fileBinaryReader.BaseStream.Seek(kerbUnloadLogonSessionTableAddr, 0);

            //Console.WriteLine("Parsing kerberos sessions");
            WalkAVLTables(minidump, (long)ptr_entry, klogonlist, template);

            return klogonlist;
        }

        private static void WalkAVLTables(MiniDump minidump, long kerbUnloadLogonSessionTableAddr, List<KerberosLogonItem> klogonlist, kerberos.KerberosTemplate template)
        {
            if (kerbUnloadLogonSessionTableAddr == 0)
                return;

            kerbUnloadLogonSessionTableAddr = Rva2offset(minidump, kerbUnloadLogonSessionTableAddr);
            minidump.BinaryReader.BaseStream.Seek(kerbUnloadLogonSessionTableAddr, 0);

            var entryBytes = minidump.BinaryReader.ReadBytes(Marshal.SizeOf(typeof(kerberos.RTL_AVL_TABLE)));
            var entry = ReadStruct<kerberos.RTL_AVL_TABLE>(entryBytes);

            //Minidump.Helpers.PrintProperties(entry);

            if (entry.OrderedPointer != 0)
            {
                var item = new KerberosLogonItem();
                long address = Rva2offset(minidump, entry.OrderedPointer);
                minidump.BinaryReader.BaseStream.Seek(address, 0);

                item.LogonSessionAddress = address;
                item.LogonSessionBytes = minidump.BinaryReader.ReadBytes(template.LogonSessionTypeSize);
                klogonlist.Add(item);
                //Minidump.Helpers.PrintProperties(item);
            }

            if (entry.BalancedRoot.RightChild != 0)
                WalkAVLTables(minidump, entry.BalancedRoot.RightChild, klogonlist, template);
            if (entry.BalancedRoot.LeftChild != 0)
                WalkAVLTables(minidump, entry.BalancedRoot.LeftChild, klogonlist, template);
        }

        public class KerberosLogonItem
        {
            public long LogonSessionAddress { get; set; }
            public byte[] LogonSessionBytes { get; set; }
        }
    }
}