using Client.Models;
using Client.Services;
using Client.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Client.ViewModels
{
    public class HttpHandlersViewModel : ViewModel
    {
        public ObservableRangeCollection<HttpHandler> Handlers { get; set; } = new();

        private HttpHandler _selectedHandler;
        public HttpHandler SelectedHandler
        {
            get { return _selectedHandler; }
            set
            {
                _selectedHandler = value;

                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public AsyncCommand CreateCommand { get; }
        public AsyncCommand EditCommand { get; }
        public AsyncCommand DeleteCommand { get; }

        private readonly SharpC2Api _api;
        private readonly SharpC2Hub _hub;

        public HttpHandlersViewModel(SharpC2Api api, SharpC2Hub hub)
        {
            _api = api;
            _hub = hub;

            _api.GetHttpHandlers().ContinueWith(AddHandlers);

            _hub.OnNotifyHttpHandlerCreated += HandlerCreated;
            _hub.OnNotifyHttpHandlerDeleted += HandlerDeleted;
            _hub.OnNotifyHttpHandlerUpdated += HandlerStateChanged;
            _hub.OnNotifyHandlerStateChanged += HandlerStateChanged;

            CreateCommand = new AsyncCommand(CreateHandler);
            EditCommand = new AsyncCommand(EditHandler, CanExecute);
            DeleteCommand = new AsyncCommand(DeleteHandler, CanExecute);
        }

        private void AddHandlers(Task<IEnumerable<HttpHandler>> task)
        {
            var handlers = task.Result;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Handlers.AddRange(handlers);
            });
        }

        private async Task CreateHandler()
        {
            await Shell.Current.GoToAsync(nameof(CreateHttpHandlerView));
        }

        private async Task EditHandler()
        {
            await Shell.Current.GoToAsync($"{nameof(EditHttpHandlerView)}?handler={SelectedHandler.Name}");
        }

        private async Task DeleteHandler()
        {
            var confirmed = await Shell.Current.DisplayAlert("Delete Handler", $"Are you sure you want to delete handler \"{SelectedHandler.Name}\"?", "Yes", "No");

            if (confirmed)
            {
                await _api.DeleteHandler(SelectedHandler.Name);
                SelectedHandler = null;
            }
        }

        private bool CanExecute(object arg)
        {
            return SelectedHandler is not null;
        }

        private async void HandlerStateChanged(string name)
        {
            var handler = await _api.GetHttpHandler(name);
            
            if (handler is null)
                return;

            var current = Handlers.FirstOrDefault(h => h.Name.Equals(name));
            var index = Handlers.IndexOf(current);

            if (index == -1)
                return;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Handlers[index] = handler;
            });
        }

        private void HandlerDeleted(string name)
        {
            var handler = Handlers.FirstOrDefault(h => h.Name.Equals(name));

            if (handler is null)
                return;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Handlers.Remove(handler);
            });
        }

        private async void HandlerCreated(string name)
        {
            var handler = await _api.GetHttpHandler(name);

            if (handler is null)
                return;

            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Handlers.Add(handler);
            });
        }
    }
}