using AutoMapper;

using SharpC2.API.V1.Responses;

using TeamServer.Handlers;
using TeamServer.Models;

namespace TeamServer.Mapping
{
    public class HandlerProfiles : Profile
    {
        public HandlerProfiles()
        {
            CreateMap<HandlerParameter, HandlerParameterResponse>();
            CreateMap<Handler, HandlerResponse>();
        }
    }
}