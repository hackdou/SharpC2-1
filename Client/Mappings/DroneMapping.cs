using AutoMapper;

using Client.Models;
using SharpC2.API.Response;

namespace Client.Mappings
{
    public class DroneMapping : Profile
    {
        public DroneMapping()
        {
            CreateMap<DroneResponse, Drone>();
        }
    }
}