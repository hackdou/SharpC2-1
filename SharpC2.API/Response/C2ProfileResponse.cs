namespace SharpC2.API.Response;

public class C2ProfileResponse
{
    public string Name { get; set; }
    public string Description { get; set; }
    public HttpOptions Http { get; set; }
    
    public class HttpOptions
    {
        public string Endpoint { get; set; }
        public int Sleep { get; set; }
        public int Jitter { get; set; }
    }
}