using Client.ViewModels;

namespace Client.Views;

public partial class CreateHttpHandlerView : ContentPage
{
	public CreateHttpHandlerView(CreateHttpHandlerViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}