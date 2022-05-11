using Client.Services;
using Client.ViewModels;

namespace Client.Views;

[QueryProperty(nameof(HandlerName), "handler")]
public partial class EditHttpHandlerView : ContentPage
{
	private readonly EditHttpHandlerViewModel _viewModel;
	private readonly SharpC2Api _api;
	
	public string HandlerName { get; set; }

	public EditHttpHandlerView(EditHttpHandlerViewModel viewModel, SharpC2Api api)
	{
		_viewModel = viewModel;
		_api = api;

		InitializeComponent();
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
    {
		base.OnAppearing();
		_viewModel.Handler = await _api.GetHttpHandler(HandlerName);
	}
}