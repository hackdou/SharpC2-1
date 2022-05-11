using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Drone.Interfaces;
using Drone.Models;

namespace Drone.Handlers;

public abstract class Handler : IHandler
{
    protected Metadata Metadata { get; private set; }
    protected IConfig Config { get; private set; }
    protected ICrypto Crypto { get; private set; }

    private readonly ConcurrentQueue<C2Message> _inbound = new();
    private readonly ConcurrentQueue<DroneTaskOutput> _outbound = new();

    public void Init(Metadata metadata, IConfig config, ICrypto crypto)
    {
        Metadata = metadata;
        Config = config;
        Crypto = crypto;
    }

    public abstract Task Start();
    public abstract void Stop();

    public bool GetInbound(out IEnumerable<C2Message> messages)
    {
        if (_inbound.IsEmpty)
        {
            messages = null;
            return false;
        }

        List<C2Message> list = new();

        while (_inbound.TryDequeue(out var message))
            list.Add(message);

        messages = list;
        return true;
    }

    public void QueueOutbound(DroneTaskOutput output)
    {
        _outbound.Enqueue(output);
    }

    protected bool GetOutbound(out C2Message outbound)
    {
        if (_outbound.IsEmpty)
        {
            outbound = null;
            return false;
        }
        
        List<DroneTaskOutput> list = new();

        while (_outbound.TryDequeue(out var output))
            list.Add(output);

        var (iv, data, checksum) = Crypto.EncryptObject(list);

        outbound = new C2Message
        {
            DroneId = Metadata.Id,
            Iv = iv,
            Data = data,
            Checksum = checksum
        };
        
        return true;
    }

    protected void QueueInbound(C2Message message)
    {
        _inbound.Enqueue(message);
    }
}