using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Response;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Drones)]
public class DronesController : ControllerBase
{
    private readonly IDroneService _drones;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public DronesController(IDroneService drones, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _drones = drones;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DroneResponse>>> GetDrones()
    {
        var drones = await _drones.GetDrones();
        var response = _mapper.Map<IEnumerable<Drone>, IEnumerable<DroneResponse>>(drones);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DroneResponse>> GetDrone(string id)
    {
        var drone = await _drones.GetDrone(id);
        if (drone is null) return NotFound();

        var response = _mapper.Map<Drone, DroneResponse>(drone);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDrone(string id)
    {
        var drone = await _drones.GetDrone(id);
        if (drone is null) return NotFound();

        await _hub.Clients.All.NotifyDroneRemoved(drone.Id);

        await _drones.DeleteDrone(drone);
        return NoContent();
    }
}