using SharpC2.Screens;

namespace SharpC2.Interfaces
{
    public interface IScreenFactory
    {
        Screen GetScreen(Screen.ScreenType type);
    }
}