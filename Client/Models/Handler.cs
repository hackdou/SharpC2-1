namespace Client.Models
{
    public class Handler
    {
        public string Name { get; set; }
        public HandlerType Type { get; set; }
        public bool Running { get; set; }
        public string Profile { get; set; }

        public enum HandlerType : int
        {
            Http = 0,
            Dns = 1,
            Tcp = 2,
            Smb = 3
        }
    }
}