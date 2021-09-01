namespace SharpC2.API.V1.Requests
{
    public class AddHostedFileRequest
    {
        public string Filename { get; set; }
        public byte[] Content { get; set; }
    }
}