using Client.Views;

namespace Client
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CreateProfileView), typeof(CreateProfileView));
            Routing.RegisterRoute(nameof(CreateHttpHandlerView), typeof(CreateHttpHandlerView));
            Routing.RegisterRoute(nameof(EditHttpHandlerView), typeof(EditHttpHandlerView));
            Routing.RegisterRoute(nameof(InteractView), typeof(InteractView));
        }
    }
}