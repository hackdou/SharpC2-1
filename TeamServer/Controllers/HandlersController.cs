using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API.V1;
using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;

using TeamServer.Handlers;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route(Routes.V1.Handlers)]
    public class HandlersController : ControllerBase
    {
        private readonly SharpC2Service _server;
        private readonly ICryptoService _crypto;
        
        private readonly IMapper _mapper;
        private readonly IHubContext<MessageHub, IMessageHub> _hub;

        public HandlersController(SharpC2Service server, IMapper mapper, IHubContext<MessageHub, IMessageHub> hub, ICryptoService crypto)
        {
            _mapper = mapper;
            _hub = hub;
            _crypto = crypto;
            _server = server;
        }

        [HttpGet]
        public IActionResult GetHandlers()
        {
            var handlers = _server.GetHandlers();
            var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HandlerResponse>>(handlers);
            
            return Ok(response);
        }

        [HttpGet("{name}")]
        public IActionResult GetHandler(string name)
        {
            var handler = _server.GetHandler(name);
            if (handler is null) return NotFound();

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            return Ok(response);
        }

        [HttpGet("types")]
        public IActionResult GetHandlerTypes()
        {
            return Ok(Enum.GetNames(typeof(CreateHandlerRequest.HandlerType)));
        }

        [HttpPost("load")]
        public async Task<IActionResult> LoadHandler([FromBody] LoadAssemblyRequest request)
        {
            var handlers = _server.LoadHandlers(request.Bytes);
            var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HandlerResponse>>(handlers);

            foreach (var handlerResponse in response)
                await _hub.Clients.All.HandlerLoaded(handlerResponse);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHandler([FromBody] CreateHandlerRequest request)
        {
            // check if handler name already exists
            var existing = _server.GetHandler(request.HandlerName);
            
            if (existing is not null)
                return BadRequest($"Handler with name '{request.HandlerName}' already exists");
            
            // create new handler
            Handler handler = request.Type switch
            {
                CreateHandlerRequest.HandlerType.HTTP => new HttpHandler(request.HandlerName),
                CreateHandlerRequest.HandlerType.SMB => new SmbHandler(request.HandlerName),
                CreateHandlerRequest.HandlerType.TCP => new TcpHandler(request.HandlerName),
                
                _ => throw new ArgumentOutOfRangeException()
            };

            // add and return path
            _server.AddHandler(handler);
            
            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Path.ToUriComponent()}";
            var path = $"{root}/{handler.Name}";

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            await _hub.Clients.All.HandlerLoaded(response);
            return Created(path, response);
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> SetHandlerParameters(string name, [FromBody] Dictionary<string, string> parameters)
        {
            var handler = _server.GetHandler(name);
            if (handler is null) return NotFound();
            handler.SetParameters(parameters);

            await _hub.Clients.All.HandlerParametersSet(parameters);

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            return Ok(response);
        }

        [HttpPatch("{name}")]
        public async Task<IActionResult> SetHandlerParameter(string name, [FromQuery] string key, [FromQuery] string value)
        {
            var handler = _server.GetHandler(name);
            if (handler is null) return NotFound();
            handler.SetParameter(key, value);

            await _hub.Clients.All.HandlerParameterSet(key, value);

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            return Ok(response);
        }

        [HttpPatch("{name}/start")]
        public async Task<IActionResult> StartHandler(string name)
        {
            var handler = _server.GetHandler(name);

            if (handler is null) return NotFound();
            if (handler.Running) return BadRequest("Handler is already running");

            handler.Init(_server, _crypto);
            var task = handler.Start();

            if (task.IsFaulted) return BadRequest(task.Exception?.Message);

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            await _hub.Clients.All.HandlerStarted(response);
            return Ok(response);
        }

        [HttpPatch("{name}/stop")]
        public async Task<IActionResult> StopHandler(string name)
        {
            var handler = _server.GetHandler(name);

            if (handler is null) return NotFound();
            if (!handler.Running) return BadRequest("Handler is already stopped");
            
            handler.Stop();

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            await _hub.Clients.All.HandlerStopped(response);
            return Ok(response);
        }

        [HttpDelete("{name}")]
        public IActionResult RemoveHandler(string name)
        {
            var handler = _server.GetHandler(name);
            if (handler is null)
                return NotFound($"Handler '{name}' not found");

            if (handler.Running)
                handler.Stop();

            _server.RemoveHandler(handler);
            
            return NoContent();
        }
    }
}