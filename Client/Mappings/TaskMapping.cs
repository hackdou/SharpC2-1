using AutoMapper;

using Client.Models;
using SharpC2.API.Response;

namespace Client.Mappings
{
    public class TaskMapping : Profile
    {
        public TaskMapping()
        {
            CreateMap<DroneTaskResponse, DroneTaskRecord>();
        }
    }
}