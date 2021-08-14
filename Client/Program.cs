using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using SharpC2.Helpers;
using SharpC2.Interfaces;
using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            PrintLogo();

            var sp = BuildServiceProvider();
            var api = sp.GetRequiredService<IApiService>();
            var factory = sp.GetRequiredService<IScreenFactory>();
            var signalR = sp.GetRequiredService<SignalRService>();
            
            var authenticated = false;

            while (!authenticated)
            {
                string server, port, nick, password;

                if (args.Contains(new string[] { "--server", "--port", "--nick", "--password" }))
                {
                    server = args.GetValue("--server");
                    port = args.GetValue("--port");
                    nick = args.GetValue("--nick");
                    password = args.GetValue("--password");
                }
                else
                {
                    server = PromptForInput("server");
                    port = PromptForInput("port");
                    nick = PromptForInput("nick");
                    password = PromptForInput("pass", true);
                }

                api.InitClient(server, port, nick, password);

                // test authentication
                var handlers = await api.GetHandlers();

                if (!handlers.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Authentication failed");
                    continue;
                }
                
                authenticated = true;
                
                // connect to SignalR
                await signalR.Connect(server, port, nick, password);
            }

            // load the drone screen as a pseudo home screen
            var screen = factory.GetScreen(Screen.ScreenType.Drones);
            screen.SetName("drones");
            await screen.LoadInitialData();
            screen.AddCommands();
            await screen.Show();
        }

        private static void PrintLogo()
        {
            Console.WriteLine(@"  ___ _                   ___ ___ ");
            Console.WriteLine(@" / __| |_  __ _ _ _ _ __ / __|_  )");
            Console.WriteLine(@" \__ \ ' \/ _` | '_| '_ \ (__ / / ");
            Console.WriteLine(@" |___/_||_\__,_|_| | .__/\___/___|");
            Console.WriteLine(@"                   |_|            ");
            Console.WriteLine(@"    @_RastaMouse                  ");
            Console.WriteLine(@"    @_xpn_                        ");
            Console.WriteLine();
        }

        private static string PromptForInput(string label, bool secure = false)
        {
            string input;
            do
            {
                input = secure
                    ? ReadLine.ReadPassword($"({label})> ")
                    : ReadLine.Read($"({label})> ");
            }
            while (string.IsNullOrEmpty(input));

            return input;
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var sp = new ServiceCollection()
                .AddSingleton<CertificateService>()
                .AddSingleton<IApiService, ApiService>()
                .AddSingleton<SignalRService>()
                .AddSingleton<IScreenFactory, ScreenFactory>()
                .AddAutoMapper(typeof(Program));

            return sp.BuildServiceProvider();
        }
    }
}