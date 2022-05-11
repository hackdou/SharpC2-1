using AutoMapper;

using Client.Models;

using RestSharp;
using RestSharp.Authenticators;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

namespace Client.Services
{
    public class SharpC2Api
    {
        private RestClient _client;
        private readonly IMapper _mapper;

        public SharpC2Api(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<string> AuthenticateUser(string hostname, string nick, string pass)
        {
            // dispose if one alreay exists
            _client?.Dispose();

            _client = new RestClient($"http://{hostname}:8008");

            var authRequest = new AuthenticationRequest
            {
                Nick = nick,
                Pass = pass
            };

            var request = new RestRequest(Routes.V1.Auth, Method.Post).AddJsonBody(authRequest);
            var authResponse = await _client.PostAsync<AuthenticationResponse>(request);

            if (authResponse.Success)
                _client.Authenticator = new JwtAuthenticator(authResponse.Token);
            
            return authResponse.Token;
        }

        public async Task<IEnumerable<C2Profile>> GetProfiles()
        {
            var request = new RestRequest(Routes.V1.Profiles);
            var response = await _client.GetAsync<IEnumerable<C2ProfileResponse>>(request);

            return _mapper.Map<IEnumerable<C2ProfileResponse>, IEnumerable<C2Profile>>(response);
        }

        public async Task<C2Profile> GetProfile(string name)
        {
            var request = new RestRequest($"{Routes.V1.Profiles}/{name}");
            var response = await _client.GetAsync<C2ProfileResponse>(request);

            return _mapper.Map<C2ProfileResponse, C2Profile>(response);
        }

        public async Task<C2Profile> CreateProfile(string yaml)
        {
            var request = new RestRequest(Routes.V1.Profiles).AddBody(yaml);
            var response = await _client.PostAsync<C2ProfileResponse>(request);

            return _mapper.Map<C2ProfileResponse, C2Profile>(response);
        }

        public async Task<C2Profile> UpdateProfile(string name, string yaml)
        {
            var request = new RestRequest($"{Routes.V1.Profiles}/{name}").AddBody(yaml);
            var response = await _client.PutAsync<C2ProfileResponse>(request);

            return _mapper.Map<C2ProfileResponse, C2Profile>(response);
        }

        public async Task DeleteProfile(string name)
        {
            var request = new RestRequest($"{Routes.V1.Profiles}/{name}");
            await _client.DeleteAsync(request);
        }

        public async Task<IEnumerable<Handler>> GetHandlers()
        {
            var request = new RestRequest(Routes.V1.Handlers);
            var response = await _client.GetAsync<IEnumerable<HandlerResponse>>(request);

            return _mapper.Map<IEnumerable<HandlerResponse>, IEnumerable<Handler>>(response);
        }

        public async Task<IEnumerable<HttpHandler>> GetHttpHandlers()
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/http");
            var response = await _client.GetAsync<IEnumerable<HttpHandlerResponse>>(request);

            return _mapper.Map<IEnumerable<HttpHandlerResponse>, IEnumerable<HttpHandler>>(response);
        }

        public async Task<HttpHandler> GetHttpHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/http/{name}", Method.Get);
            var response = await _client.GetAsync<HttpHandlerResponse>(request);

            return _mapper.Map<HttpHandlerResponse, HttpHandler>(response);
        }

        public async Task CreateHttpHandler(string name, int bindPort, string connectAddress, int connectPort, string profile)
        {
            var create = new CreateHttpHandlerRequest
            {
                Name = name,
                BindPort = bindPort,
                ConnectAddress = connectAddress,
                ConnectPort = connectPort,
                ProfileName = profile
            };

            var request = new RestRequest($"{Routes.V1.Handlers}/http", Method.Post).AddJsonBody(create);
            await _client.PostAsync(request);
        }

        public async Task UpdateHttpHandler(string name,int bindPort, string connectAddress, int connectPort, string profile)
        {
            var update = new CreateHttpHandlerRequest
            {
                BindPort = bindPort,
                ConnectAddress = connectAddress,
                ConnectPort = connectPort,
                ProfileName = profile
            };

            var request = new RestRequest($"{Routes.V1.Handlers}/http/{name}").AddJsonBody(update);
            var response = await _client.PutAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new Exception(response.Content);
        }

        public async Task ToggleHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}");
            await _client.PatchAsync<HandlerResponse>(request);
        }

        public async Task DeleteHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}");
            await _client.DeleteAsync(request);
        }

        public async Task<IEnumerable<Drone>> GetDrones()
        {
            var request = new RestRequest(Routes.V1.Drones, Method.Get);
            var response = await _client.GetAsync<IEnumerable<DroneResponse>>(request);

            return _mapper.Map<IEnumerable<DroneResponse>, IEnumerable<Drone>>(response);
        }

        public async Task<Drone> GetDrone(string id)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{id}", Method.Get);
            var response = await _client.GetAsync<DroneResponse>(request);
            
            return _mapper.Map<DroneResponse, Drone>(response);
        }

        public async Task RemoveDrone(string id)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{id}");
            await _client.DeleteAsync(request); 
        }

        public async Task<IEnumerable<DroneTaskRecord>> GetDroneTasks(string id)
        {
            var request = new RestRequest($"{Routes.V1.Tasks}/{id}", Method.Get);
            var response = await _client.GetAsync<IEnumerable<DroneTaskResponse>>(request);

            return _mapper.Map<IEnumerable<DroneTaskResponse>, IEnumerable<DroneTaskRecord>>(response);
        }

        public async Task<DroneTaskRecord> GetDroneTask(string droneId, string taskId)
        {
            var request = new RestRequest($"{Routes.V1.Tasks}/{droneId}/{taskId}", Method.Get);
            var response = await _client.GetAsync<DroneTaskResponse>(request);

            return _mapper.Map<DroneTaskResponse, DroneTaskRecord>(response);
        }

        public async Task TaskDrone(string droneId, string droneFunction, string commandAlias, string[] parameters, string artefactPath, byte[] artefact)
        {
            var task = new DroneTaskRequest
            {
                DroneId = droneId,
                DroneFunction = droneFunction,
                CommandAlias = commandAlias,
                Parameters = parameters,
                ArtefactPath = artefactPath,
                Artefact = artefact
            };

            var request = new RestRequest(Routes.V1.Tasks, Method.Post).AddJsonBody(task);
            await _client.PostAsync<HandlerResponse>(request);
        }

        public async Task<byte[]> GeneratePayload(string handler, string format)
        {
            var request = new RestRequest($"{Routes.V1.Payloads}/{handler}/{format}");
            return await _client.GetAsync<byte[]>(request);
        }
    }
}