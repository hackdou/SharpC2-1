using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MiniDumpModule.Crypto;
using MiniDumpModule.Templates;
using static MiniDumpModule.Helpers;

namespace MiniDumpModule.Decryptor
{
    internal class Credman_
    {
        public static int FindCredentials(MiniDump minidump, credman.CredmanTemplate template)
        {
            foreach (var logon in minidump.LogonList)
            {
                //Console.WriteLine("=======================");
                var credmanMem = logon.pCredentialManager;
                var luid = logon.LogonId;

                long llCurrent;
                var reference = 1;

                minidump.BinaryReader.BaseStream.Seek(credmanMem, 0);
                var credmansetBytes = minidump.BinaryReader.ReadBytes(Marshal.SizeOf(template.list_entry));

                //int list1offset = StructFieldOffset(typeof(KIWI_CREDMAN_SET_LIST_ENTRY), "list1");//bug
                long pList1 = BitConverter.ToInt64(credmansetBytes, FieldOffset<KIWI_CREDMAN_SET_LIST_ENTRY>("list1"));
                long refer = pList1 + FieldOffset<KIWI_CREDMAN_LIST_STARTER>("start");

                minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, pList1), 0);
                var credmanstarterBytes = minidump.BinaryReader.ReadBytes(Marshal.SizeOf(typeof(KIWI_CREDMAN_LIST_STARTER)));

                long pStart = BitConverter.ToInt64(credmanstarterBytes, FieldOffset<KIWI_CREDMAN_LIST_STARTER>("start"));

                if (pStart == 0)
                    continue;

                if (pStart == refer)
                    continue;

                llCurrent = pStart;
                

                do
                {
                    llCurrent = llCurrent - FieldOffset<KIWI_CREDMAN_LIST_ENTRY>("Flink");
                    llCurrent = Rva2offset(minidump, llCurrent);

                    if (llCurrent == 0)
                        continue;

                    minidump.BinaryReader.BaseStream.Seek(llCurrent, 0);
                    var entryBytes = minidump.BinaryReader.ReadBytes(Marshal.SizeOf(typeof(KIWI_CREDMAN_LIST_ENTRY)));
                    var entry = ReadStruct<KIWI_CREDMAN_LIST_ENTRY>(entryBytes);

                    var username = ExtractUnicodeStringString(minidump, entry.user);
                    var domain = ExtractUnicodeStringString(minidump, entry.server1);

                    var passDecrypted = "";

                    minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, (long)entry.encPassword), 0);

                    var msvPasswordBytes = minidump.BinaryReader.ReadBytes((int)entry.cbEncPassword);
                    var msvDecryptedPasswordBytes = BCrypt.DecryptCredentials(msvPasswordBytes, minidump.LsaKeys);

                    if (msvDecryptedPasswordBytes != null && msvDecryptedPasswordBytes.Length > 0)
                    {
                        var encoder = new UnicodeEncoding(false, false, true);
                        try
                        {
                            passDecrypted = encoder.GetString(msvDecryptedPasswordBytes);
                        }
                        catch (Exception)
                        {
                            passDecrypted = PrintHexBytes(msvDecryptedPasswordBytes);
                        }
                    }

                    if (!string.IsNullOrEmpty(username) && username.Length > 1)
                    {
                        CredMan credmanentry = new CredMan();
                        credmanentry.Reference = reference;
                        credmanentry.UserName = username;

                        if (!string.IsNullOrEmpty(domain))
                            credmanentry.DomainName = domain;
                        else
                            credmanentry.DomainName = "NULL";

                        if (!string.IsNullOrEmpty(passDecrypted))
                            credmanentry.Password = passDecrypted;
                        else
                            credmanentry.Password = "NULL";

                        if (credmanentry.Password != null)
                        {
                            var currentlogon = minidump.LogonList.FirstOrDefault(x => x.LogonId.HighPart == luid.HighPart && x.LogonId.LowPart == luid.LowPart);
                            if (currentlogon == null)
                            {
                                currentlogon = new Logon(luid);
                                //currentlogon.UserName = username;
                                currentlogon.Credman = new List<CredMan>();
                                currentlogon.Credman.Add(credmanentry);
                                minidump.LogonList.Add(currentlogon);
                            }
                            else
                            {
                                if (currentlogon.Credman == null)
                                    currentlogon.Credman = new List<CredMan>();

                                currentlogon.Credman.Add(credmanentry);
                            }
                        }
                    }
                    reference++;
                    llCurrent = entry.Flink;

                } while (llCurrent != 0 && llCurrent != refer);
            }

            return 0;
        }
    }
}