using AutoMapper;
using SharpC2.API.V1.Responses;
using TeamServer.Modules;

namespace TeamServer.Mapping
{
    public class ModuleProfiles : Profile
    {
        public ModuleProfiles()
        {
            CreateMap<Module, ModuleResponse>();
        }
    }
}