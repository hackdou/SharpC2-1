using Client.ViewModels;

namespace Client.Views;

public partial class LoginView : ContentPage
{
	public LoginView(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}