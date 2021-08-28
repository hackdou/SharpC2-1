using System;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;
using SharpC2.API.V1.Responses;

using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route(Routes.V1.Payloads)]
    public class PayloadsController : ControllerBase
    {
        private readonly IHandlerService _handlers;
        private readonly IServerService _server;
        private readonly IMapper _mapper;

        public PayloadsController(IHandlerService handlerService, IServerService serverService, IMapper mapper)
        {
            _handlers = handlerService;
            _server = serverService;
            _mapper = mapper;
        }

        [HttpGet("{handler}/{format}")]
        public async Task<IActionResult> GetPayload(string handler, string format)
        {
            var h = _handlers.GetHandler(handler);
            if (h is null) return NotFound();

            var c2 = _server.GetC2Profile();

            Payload payload = format.ToLowerInvariant() switch
            {
                "exe" => new ExePayload(h, c2),
                "dll" => new DllPayload(h, c2),
                "powershell" => new PoshPayload(h, c2),
                "raw" => new RawPayload(h, c2),
                "svc" => new ServicePayload(h, c2),
                
                _ => throw new ArgumentException("Unknown payload format")
            };
            
            await payload.Generate();
            
            var response = _mapper.Map<Payload, PayloadResponse>(payload);
            return Ok(response);
        }
    }
}