using MvvmHelpers;

namespace Client.Models
{
    public class HttpHandler : ObservableObject
    {
        public string Name { get; set; }
        public int BindPort { get; set; }
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }
        public string Profile { get; set; }
        public bool Running { get; set; }
    }
}