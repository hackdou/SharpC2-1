using Client.Models;
using Client.Services;

using MvvmHelpers.Commands;

using YamlDotNet.Serialization;

namespace Client.ViewModels
{
    public class CreateProfileViewModel : ViewModel
    {
        private readonly SharpC2Api _api;

        public AsyncCommand SaveCommand { get; }

        public CreateProfileViewModel(SharpC2Api api)
        {
            _api = api;
            SaveCommand = new AsyncCommand(CreateProfile, CanExecute);
        }

        private string _yaml;
        public string Yaml
        {
            get { return _yaml; }
            set
            {
                _yaml = value;
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task CreateProfile()
        {
            await _api.CreateProfile(Yaml);
            await Shell.Current.GoToAsync("..");
        }

        private bool CanExecute(object arg)
        {
            if (string.IsNullOrWhiteSpace(Yaml))
                return false;

            var deserializer = new Deserializer();

            try
            {
                deserializer.Deserialize<C2Profile>(Yaml);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}