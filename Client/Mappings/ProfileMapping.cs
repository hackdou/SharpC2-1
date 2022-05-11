using AutoMapper;

using Client.Models;
using SharpC2.API.Response;

namespace Client.Mappings
{
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {
            CreateMap<C2ProfileResponse, C2Profile>();
            CreateMap<C2ProfileResponse.HttpOptions, C2Profile.HttpOptions>();
        }
    }
}