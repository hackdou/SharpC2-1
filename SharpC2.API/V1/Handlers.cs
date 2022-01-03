using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;

namespace SharpC2.API.V1
{
    public partial class SharpC2Client
    {
        public async Task<IEnumerable<HandlerResponse>> GetHandlers()
        {
            var response = await _client.GetAsync(Routes.V1.Handlers);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<HandlerResponse>>(content);
        }

        public async Task<HandlerResponse> GetHandler(string name)
        {
            var response = await _client.GetAsync($"{Routes.V1.Handlers}/{name}");
            var content = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<HandlerResponse>(content);
        }

        public async Task<IEnumerable<string>> GetHandlerTypes()
        {
            var response = await _client.GetAsync($"{Routes.V1.Handlers}/types");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<string>>(content);
        }

        public async Task<HandlerResponse> CreateHandler(CreateHandlerRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(Routes.V1.Handlers, stringContent);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<HandlerResponse>(content);

            throw new HandlerException(content);
        }

        public async Task<IEnumerable<HandlerResponse>> LoadHandler(byte[] handlerBytes)
        {
            var response = await _client.PostAsync(Routes.V1.Handlers, new ByteArrayContent(handlerBytes));
            var content = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<IEnumerable<HandlerResponse>>(content); 
        }

        public async Task<HandlerResponse> SetHandlerParameter(string handlerName, string key, string value)
        {
            var method = new HttpMethod("PATCH");
            var uri = $"{Routes.V1.Handlers}/{handlerName}?key={key}&value={value}";
            var request = new HttpRequestMessage(method, uri);
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<HandlerResponse>(content); 
        }

        public async Task<HandlerResponse> StartHandler(string handlerName)
        {
            var method = new HttpMethod("PATCH");
            var uri = $"{Routes.V1.Handlers}/{handlerName}/start";
            var request = new HttpRequestMessage(method, uri);
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<HandlerResponse>(content);

            throw new HandlerException(content);
        }

        public async Task<HandlerResponse> StopHandler(string handlerName)
        {
            var method = new HttpMethod("PATCH");
            var uri = $"{Routes.V1.Handlers}/{handlerName}/stop";
            var request = new HttpRequestMessage(method, uri);
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<HandlerResponse>(content);

            throw new HandlerException(content);
        }
    }

    public class HandlerException : Exception
    {
        public HandlerException() { }

        public HandlerException(string message) : base(message) { }

        public HandlerException(string message, Exception inner) : base(message, inner) { }
    }
}