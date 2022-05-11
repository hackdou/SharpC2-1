using Client.Models;
using Client.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

using CommunityToolkit.Maui.Alerts;

using Client.Views;

namespace Client.ViewModels
{
    public class DronesViewModel : ViewModel
    {
        public ObservableRangeCollection<Drone> Drones { get; set; } = new();

        private Drone _selectedDrone;
        public Drone SelectedDrone
        {
            get { return _selectedDrone; }
            set
            {
                _selectedDrone = value;
                OnPropertyChanged();

                InteractCommand.RaiseCanExecuteChanged();
                KillCommand.RaiseCanExecuteChanged();
                RemoveCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly SharpC2Api _api;
        private readonly SharpC2Hub _hub;

        public AsyncCommand InteractCommand { get; }
        public AsyncCommand RemoveCommand { get; }
        public AsyncCommand KillCommand { get; }

        public DronesViewModel(SharpC2Api api, SharpC2Hub hub)
        {
            _api = api;
            _hub = hub;

            _api.GetDrones().ContinueWith(AddDrones);

            _hub.OnNotifyNewDrone += OnNotifyNewDrone;
            _hub.OnNotifyDroneCheckedIn += OnNotifyDroneCheckedIn;
            _hub.OnNotifyDroneRemoved += OnNotifyDroneRemoved;

            InteractCommand = new AsyncCommand(InteractDrone, CanExecute);
            RemoveCommand = new AsyncCommand(RemoveDrone, CanExecute);
            KillCommand = new AsyncCommand(KillDrone, CanExecute);
        }

        private void AddDrones(Task<IEnumerable<Drone>> task)
        {
            var drones = task.Result;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Drones.AddRange(drones);
            });
        }

        private async void OnNotifyNewDrone(string guid)
        {
            var drone = await _api.GetDrone(guid);

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Drones.Add(drone);
            });

            // show snackbar
            await Shell.Current.DisplaySnackbar($"New Drone from {drone.User} @ {drone.Hostname}.");
        }

        private async void OnNotifyDroneCheckedIn(string id)
        {
            var drone = await _api.GetDrone(id);
            if (drone is null) return;

            var current = Drones.FirstOrDefault(d => d.Id.Equals(id));

            // if first seen by client
            if (current is null)
            {
                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    Drones.Add(drone);
                });
            }
            else
            {
                // else update properties
                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    current.LastSeen = drone.LastSeen;
                });
            }
        }

        private void OnNotifyDroneRemoved(string id)
        {
            var drone = Drones.FirstOrDefault(d => d.Id.Equals(id));

            if (drone is not null)
            {
                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    Drones.Remove(drone);
                });
            }
        }

        private async Task InteractDrone()
        {
            await Shell.Current.GoToAsync($"{nameof(InteractView)}?droneId={SelectedDrone.Id}");
        }

        private async Task RemoveDrone()
        {
            await _api.RemoveDrone(SelectedDrone.Id);
            SelectedDrone = null;
        }

        private async Task KillDrone()
        {
            var confirm = await Shell.Current.DisplayAlert("Confirm", $"Kill drone {SelectedDrone.User} @ {SelectedDrone.Hostname}?", "OK", "Cancel");

            if (confirm)
                await _api.TaskDrone(SelectedDrone.Id, "exit", "exit", Array.Empty<string>(), "", Array.Empty<byte>());
        }

        private bool CanExecute(object arg)
        {
            return SelectedDrone is not null;
        }
    }
}