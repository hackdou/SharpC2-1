using Client.Models;
using Client.Services;
using Client.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Client.ViewModels
{
    public class ProfilesViewModel : ViewModel
    {
        private readonly SharpC2Api _api;
        private readonly SharpC2Hub _hub;

        public ObservableRangeCollection<C2Profile> Profiles { get; set; } = new();

        public string[] ProfilePicker
        {
            get
            {
                return Profiles.Select(p => p.Name).ToArray();
            }
        }

        private int _profileIndex;
        public int ProfileIndex
        {
            get { return _profileIndex; }
            set
            {
                _profileIndex = value;
                OnPropertyChanged(nameof(Yaml));
                SaveCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private string _yaml;
        public string Yaml
        {
            get
            {
                if (!Profiles.Any() || ProfileIndex == -1)
                    return string.Empty;

                var profile = Profiles[ProfileIndex];

                if (profile is null)
                    return string.Empty;
                
                return profile.ToString();
            }
            set
            {
                _yaml = value;
            }
        }

        public AsyncCommand CreateCommand { get; }
        public AsyncCommand SaveCommand { get; }
        public AsyncCommand DeleteCommand { get; }

        public ProfilesViewModel(SharpC2Api api, SharpC2Hub hub)
        {
            _api = api;
            _hub = hub;

            _api.GetProfiles().ContinueWith(AddProfiles);

            _hub.OnNotifyProfileCreated += NotifyProfileCreated;
            _hub.OnNotifyProfileUpdated += NotifyProfileUpdated;
            _hub.OnNotifyProfileDeleted += NotifyProfileDeleted;

            CreateCommand = new AsyncCommand(CreateProfile);
            SaveCommand = new AsyncCommand(SaveProfile, CanExecute);
            DeleteCommand = new AsyncCommand(DeleteProfile, CanExecute);
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

        private async Task CreateProfile()
        {
            await Shell.Current.GoToAsync(nameof(CreateProfileView));
        }

        private async Task SaveProfile()
        {
            try
            {
                var profile = await _api.UpdateProfile(Profiles[ProfileIndex].Name, _yaml);

                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    Profiles[ProfileIndex] = profile;
                });

                OnPropertyChanged(Yaml);

                await Shell.Current.DisplayAlert("Success", "Profile saved", "OK");
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Error", e.Message, "OK");
            }
        }

        private async Task DeleteProfile()
        {
            await _api.DeleteProfile(Profiles[ProfileIndex].Name);
        }

        private bool CanExecute(object arg)
        {
            if (!Profiles.Any() || ProfileIndex == -1)
                return false;

            var profile = Profiles[ProfileIndex];

            if (profile is null)
                return false;

            return true;
        }

        private void NotifyProfileDeleted(string name)
        {
            var profile = Profiles.FirstOrDefault(p => p.Name.Equals(name));

            if (profile is null)
                return;

            Profiles.Remove(profile);
            OnPropertyChanged(nameof(ProfilePicker));
        }

        private async void NotifyProfileUpdated(string name)
        {
            var profile = await _api.GetProfile(name);

            if (profile is null)
                return;

            var current = Profiles.FirstOrDefault(p => p.Name.Equals(name));
            
            if (current is null)
            {
                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    Profiles.Add(profile);
                });
            }
            else
            {
                var index = Profiles.IndexOf(current);

                if (index == -1)
                    return;

                Shell.Current.Dispatcher.Dispatch(() =>
                {
                    Profiles[index] = profile;
                });
            }
        }

        private async void NotifyProfileCreated(string name)
        {
            var profile = await _api.GetProfile(name);

            if (profile is null)
                return;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Profiles.Add(profile);
            });

            OnPropertyChanged(nameof(ProfilePicker));
        }
    }
}