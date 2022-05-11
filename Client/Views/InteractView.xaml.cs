using Client.Services;
using Client.ViewModels;

namespace Client.Views;

[QueryProperty(nameof(DroneId), "droneId")]
public partial class InteractView : ContentPage
{
    public string DroneId { get; set; }

    private readonly InteractViewModel _viewModel;
	private readonly SharpC2Api _api;

	public InteractView(InteractViewModel viewModel, SharpC2Api api)
	{
		_viewModel = viewModel;
		_api = api;

		InitializeComponent();
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
    {
		base.OnAppearing();

		var dt = _api.GetDrone(DroneId);
		var dr = _api.GetDroneTasks(DroneId);

		await Task.WhenAll(dt, dr);

		_viewModel.Drone = dt.Result;

        foreach (var r in dr.Result)
			_viewModel.TaskResults.Insert(0, r);
    }
}