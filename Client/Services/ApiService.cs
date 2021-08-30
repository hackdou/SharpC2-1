using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AutoMapper;

using RestSharp;

using SharpC2.API;
using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;
using SharpC2.Interfaces;
using SharpC2.Models;

namespace SharpC2.Services
{
    public class ApiService : IApiService
    {
        private readonly IMapper _mapper;
        private readonly CertificateService _certs;
        private RestClient _client;
        
        private readonly JsonSerializerOptions _options = new() {PropertyNameCaseInsensitive = true};

        public ApiService(IMapper mapper, CertificateService certs)
        {
            _mapper = mapper;
            _certs = certs;
        }
        
        public void InitClient(string hostname, string port, string nick, string pass)
        {
            _client = new RestClient($"https://{hostname}:{port}")
            {
                RemoteCertificateValidationCallback = _certs.RemoteCertificateValidationCallback
            };

            _client.AddDefaultHeader("Content-Type", "application/json");

            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{nick}:{pass}"));
            _client.AddDefaultHeader("Authorization", $"Basic {basic}");
        }

        public async Task<IEnumerable<Handler>> GetHandlers()
        {
            var request = new RestRequest(Routes.V1.Handlers, Method.GET);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful) return Array.Empty<Handler>();

            var handlers = JsonSerializer.Deserialize<IEnumerable<HandlerResponse>>(response.Content, _options);
            return _mapper.Map<IEnumerable<HandlerResponse>, IEnumerable<Handler>>(handlers);
        }

        public async Task<Handler> GetHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}", Method.GET);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful) return null;

            var handler = JsonSerializer.Deserialize<HandlerResponse>(response.Content, _options);
            return _mapper.Map<HandlerResponse, Handler>(handler);
        }

        public async Task LoadHandler(byte[] handler)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}", Method.POST);
            var asm = new LoadAssemblyRequest { Bytes = handler };
            request.AddParameter("application/json", JsonSerializer.Serialize(asm), ParameterType.RequestBody);

            await _client.ExecuteAsync(request);
        }

        public async Task SetHandlerParameter(string name, string key, string value)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}?key={key}&value={value}", Method.PATCH);
            await _client.ExecuteAsync(request);
        }

        public async Task StartHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}/start", Method.PATCH);
            await _client.ExecuteAsync(request);
        }

        public async Task StopHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}/stop", Method.PATCH);
            await _client.ExecuteAsync(request);
        }

        public async Task<byte[]> GeneratePayload(Payload payload)
        {
            var request = new RestRequest($"{Routes.V1.Payloads}/{payload.Handler}/{payload.Format}", Method.GET);
            var response = await _client.ExecuteAsync(request);
            var result = JsonSerializer.Deserialize<PayloadResponse>(response.Content, _options);

            return result?.Bytes;
        }

        public async Task<IEnumerable<Drone>> GetDrones()
        {
            var request = new RestRequest(Routes.V1.Drones, Method.GET);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful) return Array.Empty<Drone>();

            var drones = JsonSerializer.Deserialize<IEnumerable<DroneResponse>>(response.Content, _options);
            return _mapper.Map<IEnumerable<DroneResponse>, IEnumerable<Drone>>(drones);
        }

        public async Task<Drone> GetDrone(string guid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{guid}", Method.GET);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful) return null;

            var drone = JsonSerializer.Deserialize<DroneResponse>(response.Content, _options);
            return _mapper.Map<DroneResponse, Drone>(drone);
        }

        public async Task TaskDrone(string guid, string module, string command, string[] args, byte[] artefact)
        {
            var task = new DroneTaskRequest
            {
                Module = module,
                Command = command,
                Arguments = args,
                Artefact = artefact
            };
            
            var request = new RestRequest($"{Routes.V1.Drones}/{guid}/tasks", Method.POST);
            request.AddParameter("application/json", JsonSerializer.Serialize(task), ParameterType.RequestBody);

            await _client.ExecuteAsync(request);
        }

        public async Task<IEnumerable<DroneTask>> GetDroneTasks(string droneGuid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{droneGuid}/tasks", Method.GET);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful) return null;

            var drone = JsonSerializer.Deserialize<IEnumerable<DroneTaskResponse>>(response.Content, _options);
            return _mapper.Map<IEnumerable<DroneTaskResponse>, IEnumerable<DroneTask>>(drone);
        }

        public async Task<DroneTask> GetDroneTask(string droneGuid, string taskGuid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{droneGuid}/tasks/{taskGuid}", Method.GET);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful) return null;

            var drone = JsonSerializer.Deserialize<DroneTaskResponse>(response.Content, _options);
            return _mapper.Map<DroneTaskResponse, DroneTask>(drone);
        }
    }
}