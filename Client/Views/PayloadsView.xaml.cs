using Client.ViewModels;

namespace Client.Views;

public partial class PayloadsView : ContentPage
{
	public PayloadsView(PayloadsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}