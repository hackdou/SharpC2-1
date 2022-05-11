using Client.Services;
using Client.Views;

using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;

namespace Client.ViewModels
{
    public class LoginViewModel : ViewModel
    {
        public string Host { get; set; }
        public string Nick { get; set; }
        public string Pass { get; set; }

        public IAsyncCommand LoginCommand { get; }

        private readonly SharpC2Api _api;
        private readonly SharpC2Hub _hub;

        public LoginViewModel(SharpC2Api api, SharpC2Hub hub)
        {
            _api = api;
            _hub = hub;

            LoginCommand = new AsyncCommand(Login);
        }

        private async Task Login()
        {
            IsBusy = true;

            try
            {
                var token = await _api.AuthenticateUser(Host, Nick, Pass);

                if (string.IsNullOrWhiteSpace(token))
                {
                    IsBusy = false;
                    await Shell.Current.DisplayAlert("Login Failed", "", "OK");
                    return;
                }

                await _hub.Connect(Host, token);
                IsBusy = false;
                await Shell.Current.GoToAsync($"//{nameof(DronesView)}");
            }
            catch (Exception e)
            {
                IsBusy = false;
                await Shell.Current.DisplayAlert("Login Failed", e.Message, "OK");
            }
        }
    }
}