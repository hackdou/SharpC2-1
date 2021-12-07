using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CommandLine;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .MapResult(RunOptions, HandleParseErrors);
        }

        private static async Task RunOptions(Options opts)
        {
            var host = CreateHostBuilder(Array.Empty<string>()).Build();

            // set the server password for user auth
            var userService = host.Services.GetRequiredService<IUserService>();
            userService.SetPassword(opts.SharedPassword);

            var server = host.Services.GetRequiredService<SharpC2Service>();
            server.LoadDefaultModules();

            // if custom Handler
            if (!string.IsNullOrWhiteSpace(opts.HandlerPath))
            {
                var bytes = await File.ReadAllBytesAsync(opts.HandlerPath);
                server.LoadHandlers(bytes);
            }

            // if custom C2 profile
            if (!string.IsNullOrWhiteSpace(opts.ProfilePath))
            {
                var profile = await LoadC2Profile(opts.ProfilePath);
                server.SetC2Profile(profile);
            }

            await host.RunAsync();
        }

        private static async Task<C2Profile> LoadC2Profile(string path)
        {
            var text = await File.ReadAllTextAsync(path);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            return deserializer.Deserialize<C2Profile>(text);
        }

        private static async Task HandleParseErrors(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
                await Console.Error.WriteLineAsync(err?.ToString());
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureWebHostDefaults(Configure);

            return builder;
        }

        private static void Configure(IWebHostBuilder webBuilder)
            => webBuilder.UseStartup<Startup>();
    }
}