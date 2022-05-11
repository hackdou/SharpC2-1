using Client.ViewModels;

namespace Client.Views;

public partial class DronesView : ContentPage
{
	public DronesView(DronesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}