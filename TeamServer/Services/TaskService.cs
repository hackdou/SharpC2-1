using System.Diagnostics;

using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Services;

public class TaskService : ITaskService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public TaskService(IDatabaseService db, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _db = db;
        _mapper = mapper;
        _hub = hub;
    }

    public async Task AddTask(DroneTaskRecord task)
    {
        var conn = _db.GetAsyncConnection();
        
        try
        {
            var dao = _mapper.Map<DroneTaskRecord, DroneTaskRecordDao>(task);
            await conn.InsertAsync(dao);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        // finally
        // {
        //     await conn.CloseAsync();
        // }
    }

    public async Task<DroneTaskRecord> GetTask(string taskId)
    {
        var conn = _db.GetAsyncConnection();
        
        try
        {
            var dao = await conn.Table<DroneTaskRecordDao>()
                .FirstOrDefaultAsync(t => t.TaskId.Equals(taskId));
            
            var task = _mapper.Map<DroneTaskRecordDao, DroneTaskRecord>(dao);

            return task;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return null;
        }
        // finally
        // {
        //     await conn.CloseAsync();
        // }
    }

    public async Task<IEnumerable<DroneTaskRecord>> GetAllTasks()
    {
        var conn = _db.GetAsyncConnection();
        
        try
        {
            var query = conn.Table<DroneTaskRecordDao>();
            var dao = await query.ToArrayAsync();
            var tasks = _mapper.Map<IEnumerable<DroneTaskRecordDao>, IEnumerable<DroneTaskRecord>>(dao);
        
            return tasks;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return Array.Empty<DroneTaskRecord>();
        }
        // finally
        // {
        //     await conn.CloseAsync();
        // }
    }

    public async Task<IEnumerable<DroneTaskRecord>> GetTasks(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        
        try
        {
            var query = conn.Table<DroneTaskRecordDao>().Where(t => t.DroneId.Equals(droneId));
            var dao = await query.ToArrayAsync();
            var tasks = _mapper.Map<IEnumerable<DroneTaskRecordDao>, IEnumerable<DroneTaskRecord>>(dao);

            return tasks;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return Array.Empty<DroneTaskRecord>();
        }
        // finally
        // {
        //     await conn.CloseAsync();
        // }
    }

    public async Task<IEnumerable<DroneTask>> GetPendingTasks(string droneId)
    {
        var conn = _db.GetAsyncConnection();

        try
        {
            // get all pending tasks for this drone
            var query = conn.Table<DroneTaskRecordDao>()
                .Where(t => t.DroneId.Equals(droneId) && t.Status == 0);
        
            var records = await query.ToArrayAsync();

            if (records.Any())
            {
                // for each one, update the status and start time
                foreach (var record in records)
                {
                    record.Status = 1;
                    record.StartTime = DateTime.UtcNow;

                    // update db
                    await conn.UpdateAsync(record);

                    // notify hub
                    await _hub.Clients.All.NotifyDroneTaskUpdated(record.DroneId, record.TaskId);
                }
            }

            return _mapper.Map<IEnumerable<DroneTaskRecordDao>, IEnumerable<DroneTask>>(records);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return Array.Empty<DroneTask>();
        }
        // finally
        // {
        //     await conn.CloseAsync();
        // }
    }

    public async Task UpdateTasks(IEnumerable<DroneTaskOutput> outputs)
    {
        var conn = _db.GetAsyncConnection();

        try
        {
            foreach (var output in outputs)
            {
                var dao = await conn.Table<DroneTaskRecordDao>().FirstOrDefaultAsync(t =>
                    t.TaskId.Equals(output.TaskId));
            
                if (dao is null) continue;
            
                // update db
                UpdateTask(output, dao);
                await conn.UpdateAsync(dao);

                // notify hub
                await _hub.Clients.All.NotifyDroneTaskUpdated(dao.DroneId, dao.TaskId);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        // finally
        // {
        //     await conn.CloseAsync();
        // }
    }

    private static void UpdateTask(DroneTaskOutput output, DroneTaskRecordDao dao)
    {
        // update status
        dao.Status = (int)output.Status;
        
        // update result
        if (dao.Result is null || dao.Result.Length == 0)
        {
            dao.Result = output.Output;
        }
        else
        {
            var tmp = dao.Result;
            
            Array.Resize(ref tmp, tmp.Length + output.Output.Length);
            Buffer.BlockCopy(output.Output, 0, tmp, tmp.Length, output.Output.Length);

            dao.Result = tmp;
        }
        
        // set end time
        if (output.Status is DroneTaskOutput.TaskStatus.Complete or DroneTaskOutput.TaskStatus.Aborted)
            dao.EndTime = DateTime.UtcNow;
    }
}