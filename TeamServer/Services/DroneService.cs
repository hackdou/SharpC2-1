using AutoMapper;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Services;

public class DroneService : IDroneService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;

    public DroneService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task AddDrone(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.InsertAsync(dao);
    }

    public async Task<Drone> GetDrone(string id)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneDao>().FirstOrDefaultAsync(d => d.Id.Equals(id));
        var drone = _mapper.Map<DroneDao, Drone>(dao);

        return drone;
    }

    public async Task<IEnumerable<Drone>> GetDrones()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneDao>().ToArrayAsync();
        var drones = _mapper.Map<IEnumerable<DroneDao>, IEnumerable<Drone>>(dao);

        return drones;
    }

    public async Task UpdateDrone(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.UpdateAsync(dao);
    }

    public async Task DeleteDrone(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.DeleteAsync(dao);
    }
}