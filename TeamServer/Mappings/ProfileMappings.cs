using AutoMapper;
using SharpC2.API.Response;
using TeamServer.Models;

namespace TeamServer.Mappings;

public class ProfileMappings : Profile
{
    public ProfileMappings()
    {
        CreateMap<C2Profile, C2ProfileResponse>();
        CreateMap<C2Profile.HttpOptions, C2ProfileResponse.HttpOptions>();
    }
}