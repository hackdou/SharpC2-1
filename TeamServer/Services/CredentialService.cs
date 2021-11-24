using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly List<CredentialRecord> _credentials = new();

        private const string Msv = "(?s)(?<=msv :).*?(?=tspkg :)";
        private const string Wdigest = "(?s)(?<=wdigest :).*?(?=kerberos :)";
        private const string Kerberos = "(?s)(?<=kerberos :).*?(?=ssp :)";

        public void ScrapeCredentials(string input)
        {
            ParseMsvResults(Regex.Matches(input, Msv));
            ParseWdigestResults(Regex.Matches(input, Wdigest));
            ParseKerberosResults(Regex.Matches(input, Kerberos));
        }

        public void AddCredential(CredentialRecord credential)
        {
            if (string.IsNullOrEmpty(credential.Guid))
                credential.Guid = Guid.NewGuid().ToShortGuid();
            
            _credentials.Add(credential);
        }

        public CredentialRecord GetCredential(string guid)
        {
            return GetCredentials().FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<CredentialRecord> GetCredentials()
        {
            return _credentials;
        }

        public bool RemoveCredential(string guid)
        {
            var credential = GetCredential(guid);
            return _credentials.Remove(credential);
        }

        private void ParseMsvResults(MatchCollection matches)
        {
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                if (match.Value.Equals("\t\n\t")) continue;

                var split = match.Value.Split("\n\t");
                if (split.Length != 7) continue;
                var username = split[2].Remove(0, 14);
                var domain = split[3].Remove(0, 14);
                var sha1 = split[5].Remove(0, 14);
                
                AddCredential(new CredentialRecord
                {
                    Username = username,
                    Domain = domain,
                    Password = sha1,
                    Source = "Mimikatz"
                });
            }
        }

        private void ParseWdigestResults(MatchCollection matches)
        {
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                if (match.Value.Equals("\t\n\t")) continue;

                var split = match.Value.Split("\n\t");
                if (split.Length != 5) continue;
                var username = split[1].Remove(0, 14);
                var domain = split[2].Remove(0, 14);
                var password = split[3].Remove(0, 14);
                if (password.Equals("(null)")) continue;
                
                AddCredential(new CredentialRecord
                {
                    Username = username,
                    Domain = domain,
                    Password = password,
                    Source = "Mimikatz"
                });
            }
        }

        private void ParseKerberosResults(MatchCollection matches)
        {
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                if (match.Value.Equals("\t\n\t")) continue;
                
                var split = match.Value.Split("\n\t");
                if (split.Length != 5) continue;
                var username = split[1].Remove(0, 14);
                var domain = split[2].Remove(0, 14);
                var password = split[3].Remove(0, 14);
                if (password.Equals("(null)")) continue;
                
                AddCredential(new CredentialRecord
                {
                    Username = username,
                    Domain = domain,
                    Password = password,
                    Source = "Mimikatz"
                });
            }
        }
    }
}