using AutoMapper;

using Client.Models;
using SharpC2.API.Response;

namespace Client.Mappings
{
    public class HandlerMapping : Profile
    {
        public HandlerMapping()
        {
            CreateMap<HandlerResponse, Handler>();
            CreateMap<HttpHandlerResponse, HttpHandler>();
        }
    }
}