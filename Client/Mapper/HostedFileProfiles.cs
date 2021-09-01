using AutoMapper;

using SharpC2.API.V1.Responses;
using SharpC2.Models;

namespace SharpC2.Mapper
{
    public class HostedFileProfiles : Profile
    {
        public HostedFileProfiles()
        {
            CreateMap<HostedFileResponse, HostedFile>();
        }
    }
}