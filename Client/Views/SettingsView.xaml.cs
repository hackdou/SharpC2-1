using Client.Models;

namespace Client.Views;

public partial class SettingsView : ContentPage
{
	public SettingsView()
	{
		InitializeComponent();

		switch (Settings.Theme)
        {
			case 0:
				SystemButton.IsChecked = true;
				break;

			case 1:
				LightButton.IsChecked = true;
				break;

			case 2:
				DarkButton.IsChecked = true;
				break;
        }
	}

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		if (sender is not RadioButton button)
			return;

		switch (button.Content)
        {
			case "System":
				Settings.Theme = 0;
				break;

			case "Light":
				Settings.Theme = 1;
				break;

			case "Dark":
				Settings.Theme = 2;
				break;
        }

		Settings.ApplyTheme();
    }
}