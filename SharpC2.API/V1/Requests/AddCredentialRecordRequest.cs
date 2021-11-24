namespace SharpC2.API.V1.Requests
{
    public class AddCredentialRecordRequest
    {
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
    }
}