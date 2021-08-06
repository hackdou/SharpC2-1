using System.Collections.Generic;

namespace Drone
{
    public class DroneConfig
    {
        private readonly Dictionary<string, object> _configs = new();

        public void SetConfig(string name, object value)
        {
            if (!_configs.ContainsKey(name)) _configs.Add(name, null);
            _configs[name] = value;
        }

        public T GetConfig<T>(string name)
        {
            if (!_configs.ContainsKey(name)) return default;
            return (T) _configs[name];
        }
    }
}