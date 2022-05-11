using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class TaskMapping : Profile
{
    public TaskMapping()
    {
        CreateMap<DroneTaskRecord, DroneTaskRecordDao>()
            .ForMember(t =>
                t.Parameters, o =>
                o.MapFrom(t => string.Join("__,__", t.Parameters)));

        CreateMap<DroneTaskRecordDao, DroneTaskRecord>()
            .ForMember(t =>
                t.Parameters, o =>
                o.MapFrom(t => t.Parameters.Split("__,__", StringSplitOptions.RemoveEmptyEntries)));

        CreateMap<DroneTaskRecord, DroneTaskResponse>();
        
        CreateMap<DroneTaskRecordDao, DroneTask>()
            .ForMember(t =>
                t.Parameters, o =>
                o.MapFrom(t => t.Parameters.Split("__,__", StringSplitOptions.RemoveEmptyEntries)));
    }
}