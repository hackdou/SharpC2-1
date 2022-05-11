using Client.Models;
using Client.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Client.ViewModels
{
    public class CreateHttpHandlerViewModel : ViewModel
    {
        public ObservableRangeCollection<C2Profile> Profiles { get; set; } = new();

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private int _bindPort;
        public int BindPort
        {
            get { return _bindPort; }
            set
            {
                _bindPort = value;
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string _connectAddress;
        public string ConnectAddress
        {
            get { return _connectAddress; }
            set
            {
                _connectAddress = value;
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private int _connectPort;
        public int ConnectPort
        {
            get { return _connectPort; }
            set
            {
                _connectPort = value;
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        public string[] ProfilePicker
        {
            get
            {
                return Profiles.Select(p => p.Name).ToArray();
            }
        }

        private string _selectedProfile;
        public string SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                _selectedProfile = value;
                CreateCommand.RaiseCanExecuteChanged();
            }
        }


        public AsyncCommand CreateCommand { get; }

        private readonly SharpC2Api _api;

        public CreateHttpHandlerViewModel(SharpC2Api api)
        {
            _api = api;
            _api.GetProfiles().ContinueWith(AddProfiles);

            CreateCommand = new AsyncCommand(CreateHandler, CanExecute);
        }

        private void AddProfiles(Task<IEnumerable<C2Profile>> task)
        {
            var profiles = task.Result;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Profiles.AddRange(profiles);
            });

            OnPropertyChanged(nameof(ProfilePicker));
        }

        private async Task CreateHandler()
        {
            IsBusy = true;

            try
            {
                await _api.CreateHttpHandler(Name, BindPort, ConnectAddress, ConnectPort, SelectedProfile);
                IsBusy = false;

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception e)
            {
                IsBusy = false;
                await Shell.Current.DisplayAlert("Create Handler Failed", e.Message, "OK");
            }
        }

        private bool CanExecute(object arg)
        {
            return !string.IsNullOrWhiteSpace(Name)
                && BindPort is > 0 and <= 65535
                && !string.IsNullOrWhiteSpace(ConnectAddress)
                && ConnectPort is > 0 and <= 65535
                && !string.IsNullOrWhiteSpace(SelectedProfile);
        }
    }
}