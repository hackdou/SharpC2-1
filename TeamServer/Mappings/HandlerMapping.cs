using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Handlers;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class HandlerMapping : Profile
{
    public HandlerMapping()
    {
        CreateMap<HttpHandler, HttpHandlerDao>()
            .ForMember(h => h.Profile, o => o.MapFrom(h => h.Profile.Name))
            .IncludeAllDerived();

        CreateMap<Handler, HandlerResponse>()
            .ForMember(h => h.Profile, o => o.MapFrom(h => h.Profile.Name));
            
        CreateMap<HttpHandler, HttpHandlerResponse>()
            .ForMember(h => h.Profile, o => o.MapFrom(h => h.Profile.Name));
    }
}