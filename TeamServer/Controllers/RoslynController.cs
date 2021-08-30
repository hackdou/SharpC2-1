using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using SharpC2.API;
using SharpC2.API.V1.Responses;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route(Routes.V1.SharpShell)]
    public class RoslynController : ControllerBase
    {
        private readonly IDroneService _drones;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessageHub, IMessageHub> _hub;

        public RoslynController(IDroneService drones, IMapper mapper, IHubContext<MessageHub, IMessageHub> hub)
        {
            _drones = drones;
            _mapper = mapper;
            _hub = hub;
        }

        [HttpPost("{droneGuid}")]
        public async Task<IActionResult> TaskDrone(string droneGuid, [FromBody] string code)
        {
            var drone = _drones.GetDrone(droneGuid);
            if (drone is null) return NotFound();

            if (string.IsNullOrEmpty(code)) return BadRequest("Code cannot be empty");

            byte[] assembly;

            await using (var ms = new MemoryStream())
            {
                var result = CompileCode(code, ms);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(d => 
                        d.IsWarningAsError || 
                        d.Severity == DiagnosticSeverity.Error);

                    return BadRequest(failures);
                }
                
                ms.Seek(0, SeekOrigin.Begin);
                assembly = ms.ToArray();
            }

            var task = new DroneTask("stdapi", "sharpshell")
            {
                Arguments = new[] {"SharpShell", "Execute"},
                Artefact = assembly
            };
            
            drone.TaskDrone(task);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Path.ToUriComponent()}";
            var path = $"{root}/{task.TaskGuid}";

            var response = _mapper.Map<DroneTask, DroneTaskResponse>(task);
            await _hub.Clients.All.DroneTasked(droneGuid, task.TaskGuid);
            return Created(path, response);
        }

        private EmitResult CompileCode(string code, Stream stream)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();
            
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return compilation.Emit(stream);
        }
    }
}