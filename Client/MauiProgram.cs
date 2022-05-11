using Client.Views;
using Client.ViewModels;
using Client.Services;

using CommunityToolkit.Maui;

namespace Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(ConfigureFonts);

            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<DronesView>();
            builder.Services.AddTransient<DronesViewModel>();

            builder.Services.AddTransient<InteractView>();
            builder.Services.AddTransient<InteractViewModel>();

            builder.Services.AddTransient<ProfilesView>();
            builder.Services.AddTransient<ProfilesViewModel>();

            builder.Services.AddTransient<CreateProfileView>();
            builder.Services.AddTransient<CreateProfileViewModel>();

            builder.Services.AddTransient<HttpHandlersView>();
            builder.Services.AddTransient<HttpHandlersViewModel>();

            builder.Services.AddTransient<PayloadsView>();
            builder.Services.AddTransient<PayloadsViewModel>();

            builder.Services.AddTransient<CreateHttpHandlerView>();
            builder.Services.AddTransient<CreateHttpHandlerViewModel>();

            builder.Services.AddTransient<EditHttpHandlerView>();
            builder.Services.AddTransient<EditHttpHandlerViewModel>();

            builder.Services.AddTransient<SettingsView>();

            builder.Services.AddSingleton<SharpC2Api>();
            builder.Services.AddSingleton<SharpC2Hub>();
            builder.Services.AddTransient<SharpC2Commands>();

            builder.Services.AddAutoMapper(typeof(MauiProgram));

            return builder.Build();
        }

        private static void ConfigureFonts(IFontCollection fonts)
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("CourierNew-Regular.ttf", "CourierNewRegular");
        }
    }
}