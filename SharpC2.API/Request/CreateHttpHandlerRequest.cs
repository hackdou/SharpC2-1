namespace SharpC2.API.Request;

public class CreateHttpHandlerRequest
{
    public string Name { get; set; }
    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public string ProfileName { get; set; }
}