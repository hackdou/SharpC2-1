using Client.ViewModels;

namespace Client.Views;

public partial class CreateProfileView : ContentPage
{
	public CreateProfileView(CreateProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}