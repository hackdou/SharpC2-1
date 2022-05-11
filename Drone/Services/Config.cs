using System.Collections.Generic;
using Drone.Interfaces;

namespace Drone.Services;

public class Config : IConfig
{
    private readonly Dictionary<string, object> _configs = new();

    public void Set(string key, object value)
    {
        if (!_configs.ContainsKey(key))
            _configs.Add(key, null);

        _configs[key] = value;
    }

    public T Get<T>(string key)
    {
        if (!_configs.ContainsKey(key))
            return default;

        return (T)_configs[key];
    }
}