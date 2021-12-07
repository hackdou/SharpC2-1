using System;
using System.Linq;
using System.Threading.Tasks;

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
    [Route(Routes.V1.HostedFiles)]
    public class HostedFilesController : ControllerBase
    {
        private readonly SharpC2Service _server;
        private readonly IHubContext<MessageHub, IMessageHub> _hub;

        public HostedFilesController(SharpC2Service server, IHubContext<MessageHub, IMessageHub> hub)
        {
            _server = server;
            _hub = hub;
        }

        [HttpGet("{handlerName}")]
        public IActionResult GetHostedFiles(string handlerName)
        {
            var handler = (HttpHandler) _server.GetHandler(handlerName);
            var fileInfos = handler.GetHostedFiles();
            var response = fileInfos.Select(fileInfo =>
                new HostedFileResponse { Filename = fileInfo.Name, Size = fileInfo.Length }).ToList();

            return Ok(response);
        }

        [HttpPost("{handlerName}")]
        public async Task<IActionResult> UploadFile(string handlerName, [FromBody] AddHostedFileRequest request)
        {
            var handler = (HttpHandler) _server.GetHandler(handlerName);

            if (!handler.Running)
            {
                handler.Init(_server);
                var task = handler.Start();
                if (task.IsFaulted) return BadRequest(task.Exception?.Message);
            }

            await handler.AddHostedFile(request.Content, request.Filename);
            
            var connectAddress = handler.Parameters.Single(p =>
                p.Name.Equals("ConnectAddress", StringComparison.OrdinalIgnoreCase)).Value;
            
            var bindPort = handler.Parameters.Single(p =>
                p.Name.Equals("BindPort", StringComparison.OrdinalIgnoreCase)).Value;

            var path = $"http://{connectAddress}:{bindPort}/{request.Filename}";
            await _hub.Clients.All.HostedFileAdded(request.Filename);
            return Created(path, request.Content);
        }

        [HttpDelete("{handlerName}/{filename}")]
        public async Task<IActionResult> DeleteFile(string handlerName, string filename)
        {
            var handler = (HttpHandler)_server.GetHandler(handlerName);
            if (!handler.RemoveHostedFile(filename)) return NotFound();

            await _hub.Clients.All.HostedFileDeleted(filename);
            return NoContent();
        }
    }
}