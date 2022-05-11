using Client.ViewModels;

namespace Client.Views;

public partial class ProfilesView : ContentPage
{
	public ProfilesView(ProfilesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}