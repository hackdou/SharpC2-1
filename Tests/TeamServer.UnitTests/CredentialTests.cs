using System;

using TeamServer.Interfaces;
using TeamServer.Models;

using Xunit;

namespace TeamServer.UnitTests
{
    public class CredentialTests
    {
        private readonly ICredentialService _credentials;

        public CredentialTests(ICredentialService credentials)
        {
            _credentials = credentials;
        }

        [Fact]
        public void AddCredential()
        {
            var oCred = new CredentialRecord
            {
                Guid = Guid.NewGuid().ToShortGuid(),
                Username = "RastaMouse",
                Domain = "TEST",
                Password = "Passw0rd!",
                Source = "Manual"
            };
                
            _credentials.AddCredential(oCred);

            var cred = _credentials.GetCredential(oCred.Guid);
            
            Assert.NotNull(cred);
            Assert.Equal(oCred.Username, cred.Username);
            Assert.Equal(oCred.Domain, cred.Domain);
            Assert.Equal(oCred.Password, cred.Password);
            Assert.Equal(oCred.Source, cred.Source);
        }

        [Fact]
        public void RemoveCredential()
        {
            var oCred = new CredentialRecord
            {
                Guid = Guid.NewGuid().ToShortGuid(),
                Username = "RastaMouse",
                Domain = "TEST",
                Password = "Passw0rd!",
                Source = "Manual"
            };
                
            _credentials.AddCredential(oCred);
            _credentials.RemoveCredential(oCred.Guid);

            var cred = _credentials.GetCredential(oCred.Guid);
            Assert.Null(cred);
        }
    }
}