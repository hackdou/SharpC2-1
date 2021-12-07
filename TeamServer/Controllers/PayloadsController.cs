using System;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API.V1;
using SharpC2.API.V1.Responses;

using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route(Routes.V1.Payloads)]
    public class PayloadsController : ControllerBase
    {
        private readonly SharpC2Service _server;
        private readonly IMapper _mapper;

        public PayloadsController(SharpC2Service server, IMapper mapper)
        {
            _server = server;
            _mapper = mapper;
        }

        [HttpGet("formats")]
        public IActionResult GetPayloadFormats()
        {
            return Ok(Enum.GetNames(typeof(SharpC2Service.PayloadFormat)));
        }

        [HttpGet("{handler}/{format}")]
        public async Task<IActionResult> GetPayload(string handler, string format)
        {
            // get the handler
            var h = _server.GetHandler(handler);
            if (h is null) return NotFound("Handler not found");

            // parse the format
            if (!Enum.TryParse(format, true, out SharpC2Service.PayloadFormat payloadFormat))
                return BadRequest("Invalid payload format");

            var payload = await _server.GeneratePayload(payloadFormat, h);
            
            var response = _mapper.Map<Payload, PayloadResponse>(payload);
            return Ok(response);
        }
    }
}