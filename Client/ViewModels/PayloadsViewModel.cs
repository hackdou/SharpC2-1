using Client.Models;
using Client.Services;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Client.ViewModels
{
    public class PayloadsViewModel : ViewModel
    {
        public ObservableRangeCollection<Handler> Handlers { get; set; } = new();

        public string[] HandlerPicker
        {
            get
            {
                return Handlers.Select(h => h.Name).ToArray();
            }
        }

        private string _selectedHandler;
        public string SelectedHandler
        {
            get { return _selectedHandler; }
            set
            {
                _selectedHandler = value;
                GeneratePayloadCommand.RaiseCanExecuteChanged();
            }
        }

        public string[] Formats
        {
            get
            {
                return new string[] { "Exe", "Dll", "ServiceExe", "PowerShell", "Shellcode" };
            }
        }

        private string _selectedFormat;
        public string SelectedFormat
        {
            get { return _selectedFormat; }
            set
            {
                _selectedFormat = value;
                GeneratePayloadCommand.RaiseCanExecuteChanged();
            }
        }

        private string _location;
        public string Location
        {
            get { return _location; }
            set
            {
                _location = value;
                GeneratePayloadCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly SharpC2Api _api;

        public AsyncCommand GeneratePayloadCommand { get; }

        public PayloadsViewModel(SharpC2Api api)
        {
            _api = api;
            _api.GetHandlers().ContinueWith(AddHandlers);

            GeneratePayloadCommand = new AsyncCommand(GeneratePayload, CanGenerate);
        }

        private void AddHandlers(Task<IEnumerable<Handler>> task)
        {
            var handlers = task.Result;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Handlers.AddRange(handlers);
            });

            OnPropertyChanged(nameof(HandlerPicker));
        }

        public async Task GeneratePayload()
        {
            IsBusy = true;
            var payload = await _api.GeneratePayload(SelectedHandler, SelectedFormat);

            if (payload is null || payload.Length == 0)
            {
                await Shell.Current.DisplayAlert("Failed", "Failed to generate payload", "OK");
            }
            else
            {
                try
                {
                    await File.WriteAllBytesAsync(Location, payload);
                    await Shell.Current.DisplayAlert("Complete", $"Saved {payload.Length} bytes to {Location}", "OK");
                }
                catch (Exception e)
                {
                    await Shell.Current.DisplayAlert("Failed", e.Message, "OK");
                }
                
                IsBusy = false;                
            }
        }

        public bool CanGenerate(object arg)
        {
            if (string.IsNullOrWhiteSpace(SelectedHandler))
                return false;

            if (string.IsNullOrWhiteSpace(SelectedFormat))
                return false;

            if (string.IsNullOrWhiteSpace(Location))
                return false;

            var directory = Path.GetDirectoryName(Location);
            if (!Directory.Exists(directory))
                return false;

            return true;
        }
    }
}