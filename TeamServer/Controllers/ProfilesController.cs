using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharpC2.API;
using SharpC2.API.Response;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

using YamlDotNet.Serialization;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Profiles)]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profiles;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public ProfilesController(IProfileService profiles, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _profiles = profiles;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<C2ProfileResponse>>> GetProfiles()
    {
        var profiles = await _profiles.GetProfiles();
        var response = _mapper.Map<IEnumerable<C2Profile>, IEnumerable<C2ProfileResponse>>(profiles);

        return Ok(response);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<C2ProfileResponse>> GetProfile(string name)
    {
        var profile = await _profiles.GetProfile(name);

        if (profile is null)
            return NotFound();

        var response = _mapper.Map<C2Profile, C2ProfileResponse>(profile);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<C2ProfileResponse>> CreateProfile([FromBody] string yaml)
    {
        var deserializer = new Deserializer();
        var profile = deserializer.Deserialize<C2Profile>(yaml);
        
        await _profiles.AddProfile(profile);
        await _hub.Clients.All.NotifyProfileCreated(profile.Name);

        var response = _mapper.Map<C2Profile, C2ProfileResponse>(profile);
        return Ok(response);
    }

    [HttpPut("{name}")]
    public async Task<ActionResult<C2ProfileResponse>> UpdateProfile(string name, [FromBody] string yaml)
    {
        var deserializer = new Deserializer();
        var profile = deserializer.Deserialize<C2Profile>(yaml);

        // don't want to change the name
        profile.Name = name;
        
        await _profiles.UpdateProfile(profile);
        profile = await _profiles.GetProfile(name);

        await _hub.Clients.All.NotifyProfileUpdated(name);

        var response = _mapper.Map<C2Profile, C2ProfileResponse>(profile);
        return Ok(response);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteProfile(string name)
    {
        var profile = await _profiles.GetProfile(name);

        if (profile is null)
            return NotFound();

        await _profiles.DeleteProfile(profile);
        await _hub.Clients.All.NotifyProfileDeleted(name);
        
        return NoContent();
    }
}