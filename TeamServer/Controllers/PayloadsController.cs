using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;

using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Payloads)]
public class PayloadsController : ControllerBase
{
    private readonly IHandlerService _handlers;
    private readonly IPayloadService _payloads;

    public PayloadsController(IHandlerService handlers, IPayloadService payloads)
    {
        _handlers = handlers;
        _payloads = payloads;
    }

    [HttpGet("{handler}/{format}")]
    public async Task<ActionResult<byte[]>> GeneratePayload(string handler, string format)
    {
        var h = _handlers.GetHandler(handler);

        if (h is null)
            return NotFound("Handler not found");

        if (!Enum.TryParse(format, true, out PayloadFormat f))
            return BadRequest("Invalid payload format");

        return await _payloads.GeneratePayload(h, f);
    }
}