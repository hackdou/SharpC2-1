using System.Collections.Generic;

namespace SharpC2.API.V1.Responses
{
    public class HandlerResponse
    {
        public string Name { get; set; }
        public IEnumerable<HandlerParameterResponse> Parameters { get; set; }
        public bool Running { get; set; }
    }
}