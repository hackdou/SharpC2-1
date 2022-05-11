using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Services
{
    public class SharpC2Hub
    {
        private HubConnection _connection;

        // events
        public event Action<string> OnNotifyProfileCreated;
        public event Action<string> OnNotifyProfileUpdated;
        public event Action<string> OnNotifyProfileDeleted;

        public event Action<string> OnNotifyHttpHandlerCreated;
        public event Action<string> OnNotifyHttpHandlerDeleted;
        public event Action<string> OnNotifyHttpHandlerUpdated;
        public event Action<string> OnNotifyHandlerStateChanged;

        public event Action<string> OnNotifyNewDrone;
        public event Action<string> OnNotifyDroneCheckedIn;
        public event Action<string> OnNotifyDroneRemoved;

        public event Action<string, string> OnNotifyDroneTaskUpdated;

        public async Task Connect(string hostname, string bearer)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"http://{hostname}:8008/SharpC2", o =>
                {
                    o.AccessTokenProvider = () => Task.FromResult(bearer);
                })
                .WithAutomaticReconnect()
                .Build();

            await _connection.StartAsync();

            // regsiter events
            _connection.On<string>("NotifyProfileCreated", NotifyProfileCreated);
            _connection.On<string>("NotifyProfileUpdated", NotifyProfileUpdated);
            _connection.On<string>("NotifyProfileDeleted", NotifyProfileDeleted);

            _connection.On<string>("NotifyHttpHandlerCreated", NotifyHttpHandlerCreated);
            _connection.On<string>("NotifyHttpHandlerDeleted", NotifyHttpHandlerDeleted);
            _connection.On<string>("NotifyHttpHandlerUpdated", NotifyHttpHandlerUpdated);
            _connection.On<string>("NotifyHandlerStateChanged", NotifyHandlerStateChanged);

            _connection.On<string>("NotifyNewDrone", NotifyNewDrone);
            _connection.On<string>("NotifyDroneCheckedIn", NotifyDroneCheckedIn);
            _connection.On<string>("NotifyDroneRemoved", NotifyDroneRemoved);

            _connection.On<string, string>("NotifyDroneTaskUpdated", NotifyDroneTaskUpdated);
        }

        private void NotifyProfileCreated(string name) => OnNotifyProfileCreated?.Invoke(name);
        private void NotifyProfileUpdated(string name) => OnNotifyProfileUpdated?.Invoke(name);
        private void NotifyProfileDeleted(string name) => OnNotifyProfileDeleted?.Invoke(name);

        private void NotifyHttpHandlerCreated(string name) => OnNotifyHttpHandlerCreated?.Invoke(name);
        private void NotifyHttpHandlerDeleted(string name) => OnNotifyHttpHandlerDeleted?.Invoke(name);
        private void NotifyHttpHandlerUpdated(string name) => OnNotifyHttpHandlerUpdated?.Invoke(name);
        private void NotifyHandlerStateChanged(string name) => OnNotifyHandlerStateChanged?.Invoke(name);
        
        private void NotifyNewDrone(string name) => OnNotifyNewDrone?.Invoke(name);
        private void NotifyDroneCheckedIn(string name) => OnNotifyDroneCheckedIn?.Invoke(name);
        private void NotifyDroneRemoved(string name) => OnNotifyDroneRemoved?.Invoke(name);
        
        private void NotifyDroneTaskUpdated(string droneId, string taskId) => OnNotifyDroneTaskUpdated?.Invoke(droneId, taskId);
    }
}