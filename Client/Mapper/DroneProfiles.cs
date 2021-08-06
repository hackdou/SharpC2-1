using AutoMapper;

using SharpC2.API.V1.Responses;
using SharpC2.Models;

namespace SharpC2.Mapper
{
    public class DroneProfiles : Profile
    {
        public DroneProfiles()
        {
            CreateMap<DroneResponse, Drone>();
            CreateMap<DroneTaskResponse, DroneTask>();
            
            CreateMap<DroneModuleResponse, DroneModule>();
            CreateMap<DroneModuleResponse.CommandResponse, DroneModule.Command>();
            CreateMap<DroneModuleResponse.CommandResponse.ArgumentResponse, DroneModule.Command.Argument>();
        }
    }
}