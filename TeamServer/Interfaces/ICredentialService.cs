using System.Collections.Generic;

using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface ICredentialService
    {
        void ScrapeCredentials(string input);

        void AddCredential(CredentialRecord credential);
        CredentialRecord GetCredential(string guid);
        IEnumerable<CredentialRecord> GetCredentials();
        bool RemoveCredential(string guid);
    }
}