using Client.Models;
using Client.Services;
using Client.ViewModels;

namespace Client.Views;

public partial class HttpHandlersView : ContentPage
{
	private readonly SharpC2Api _api;

	public HttpHandlersView(HttpHandlersViewModel viewModel, SharpC2Api api)
	{
		_api = api;

		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void Switch_Toggled(object sender, ToggledEventArgs e)
    {
		if (sender is not Switch sw)
			return;

		if (sw.BindingContext is not HttpHandler handler)
			return;

		// get current state
		var current = await _api.GetHttpHandler(handler.Name);

		// if states are the same, do nothing
		if (current.Running == e.Value)
			return;

		// otherwise toggle the api
		await _api.ToggleHandler(handler.Name);
	}
}