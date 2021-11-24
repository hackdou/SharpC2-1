namespace TeamServer.Models
{
    public class CredentialRecord
    {
        public string Guid { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
    }
}