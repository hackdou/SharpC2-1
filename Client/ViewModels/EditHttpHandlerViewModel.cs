using Client.Models;
using Client.Services;

using MvvmHelpers.Commands;

namespace Client.ViewModels
{
    [QueryProperty(nameof(Name), "handler")]
    public class EditHttpHandlerViewModel : ViewModel
    {
        public string Name { get; set; }

        private HttpHandler _handler;
        public HttpHandler Handler
        {
            get { return _handler; }
            set
            {
                _handler = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand SaveCommand { get; }

        private readonly SharpC2Api _api;

        public EditHttpHandlerViewModel(SharpC2Api api)
        {
            _api = api;

            SaveCommand = new AsyncCommand(SaveHandler);
        }

        private async Task SaveHandler()
        {
            try
            {
                await _api.UpdateHttpHandler(Handler.Name, Handler.BindPort, Handler.ConnectAddress, Handler.ConnectPort, Handler.Profile);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Error", e.Message, "OK");
            }
        }
    }
}