using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class DroneMapping : Profile
{
    public DroneMapping()
    {
        CreateMap<Drone, DroneDao>();
        CreateMap<DroneDao, Drone>();
        CreateMap<Drone, DroneResponse>();
    }
}