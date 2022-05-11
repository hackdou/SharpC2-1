using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Services;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Handlers)]
public class HandlersController : ControllerBase
{
    private readonly IHandlerService _handlers;
    private readonly IDatabaseService _database;
    private readonly IProfileService _profiles;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public HandlersController(IHandlerService handlers, IDatabaseService database, IProfileService profiles,
        IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _handlers = handlers;
        _database = database;
        _profiles = profiles;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public ActionResult<IEnumerable<HandlerResponse>> GetHandlers()
    {
        var handlers = _handlers.GetHandlers();
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HandlerResponse>>(handlers);

        return Ok(response);
    }

    [HttpGet("http")]
    public ActionResult<IEnumerable<HttpHandlerResponse>> GetHttpHandlers()
    {
        var handlers = _handlers.GetHandlers().Where(h => h.Type == Handler.HandlerType.Http);
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HttpHandlerResponse>>(handlers);

        return Ok(response);
    }

    [HttpGet("http/{name}")]
    public ActionResult<HttpHandlerResponse> GetHttpHandler(string name)
    {
        var handler = _handlers.GetHandler(name);
        if (handler is null) return NotFound();

        var response = _mapper.Map<Handler, HttpHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpPost("http")]
    public async Task<ActionResult<HttpHandlerResponse>> CreateHttpHandler([FromBody] CreateHttpHandlerRequest request)
    {
        // find profile
        var profile = await _profiles.GetProfile(request.ProfileName);

        if (profile is null)
            return NotFound("Profile not found");
        
        // create and start
        var handler = new HttpHandler(request.Name, request.BindPort, request.ConnectAddress, request.ConnectPort, profile);
        handler.Init(_database, _hub);
        await handler.Start();
        
        // store
        await _handlers.AddHandler(handler);
        
        // notify hub
        await _hub.Clients.All.NotifyHttpHandlerCreated(handler.Name);

        // return to user
        var response = _mapper.Map<HttpHandler, HttpHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpPut("http/{name}")]
    public async Task<ActionResult<HandlerResponse>> UpdateHttpHandler(string name, [FromBody] CreateHttpHandlerRequest request)
    {
        var handler = (HttpHandler) _handlers.GetHandler(name);
        
        if (handler is null)
            return NotFound("Handler not found");

        var profile = await _profiles.GetProfile(request.ProfileName);

        if (profile is null)
            return NotFound("Profile not found");

        // stop
        handler.Stop();

        // set new values
        handler.BindPort = request.BindPort;
        handler.ConnectAddress = request.ConnectAddress;
        handler.ConnectPort = request.ConnectPort;
        handler.Profile = profile;

        // start
        await handler.Start();

        // notify hub
        await _hub.Clients.All.NotifyHttpHandlerUpdated(handler.Name);

        // response
        var response = _mapper.Map<HttpHandler, HttpHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpPatch("{name}")]
    public async Task<ActionResult<HandlerResponse>> ToggleHandlerStatus(string name)
    {
        // get handler
        var handler = _handlers.GetHandler(name);
        
        if (handler is null)
            return NotFound();

        // toggle status
        if (handler.Running) handler.Stop();
        else await handler.Start();
        
        // notify hub
        await _hub.Clients.All.NotifyHandlerStateChanged(handler.Name);
        
        // return response
        var response = _mapper.Map<Handler, HandlerResponse>(handler);
        return Ok(response);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteHandler(string name)
    {
        var handler = _handlers.GetHandler(name);
        
        if (handler is null)
            return NotFound();
        
        handler.Stop();
        
        await _hub.Clients.All.NotifyHttpHandlerDeleted(handler.Name);
        await _handlers.DeleteHandler(handler);
        
        return NoContent();
    }
}