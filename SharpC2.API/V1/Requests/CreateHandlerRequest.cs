namespace SharpC2.API.V1.Requests
{
    public class CreateHandlerRequest
    {
        public string HandlerName { get; set; }
        public HandlerType Type { get; set; }

        public enum HandlerType
        {
            HTTP,
            SMB,
            TCP
        }
    }
}