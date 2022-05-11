using Client.Models;

namespace Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Settings.ApplyTheme();
        MainPage = new AppShell();
    }
}