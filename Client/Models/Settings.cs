namespace Client.Models;

public static class Settings
{
    // 0 = system
    // 1 = light
    // 2 = dark
    private const int _theme = 0;

    public static int Theme
    {
        get => Preferences.Get(nameof(Theme), _theme);
        set => Preferences.Set(nameof(Theme), value);
    }

    public static void ApplyTheme()
    {
        switch (Theme)
        {
            case 0:
                App.Current.UserAppTheme = AppTheme.Unspecified;
                break;

            case 1:
                App.Current.UserAppTheme = AppTheme.Light;
                break;

            case 2:
                App.Current.UserAppTheme = AppTheme.Dark;
                break;
        }
    }
}