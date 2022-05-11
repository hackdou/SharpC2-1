namespace SharpC2.API.Response;

public class HttpHandlerResponse
{
    public string Name { get; set; }
    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public bool Running { get; set; }
    public string Profile { get; set; }
}