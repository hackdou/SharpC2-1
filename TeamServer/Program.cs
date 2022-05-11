using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using TeamServer.Filters;
using TeamServer.Interfaces;
using TeamServer.Services;

namespace TeamServer;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // get password on cmdline
        var password = string.Empty;
        while (string.IsNullOrWhiteSpace(password))
        {
            Console.Write("Set Password > ");
            password = ReadPassword();
            Console.Write(Environment.NewLine);
        }
        
        // initialise auth service
        var authService = new AuthenticationService();
        builder.Services.AddSingleton<IAuthenticationService>(authService);
        
        // set password on auth service
        var jwtKey = GenerateJwtKey(password);
        authService.SetServerPassword(password, jwtKey);
        
        // configure jwt auth
        builder.Services
            .AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(j =>
            {
                j.SaveToken = true;
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
                };
            });

        builder.Services.AddControllers(ConfigureControllers);
        builder.Services.AddEndpointsApiExplorer();
        
        // ensure swagger can auth as well
        builder.Services.AddSwaggerGen(s =>
        {
            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n"
            });
            s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        // hub
        builder.Services.AddSignalR();

        // SharpC2 services
        builder.Services.AddSingleton<IProfileService, ProfileService>();
        builder.Services.AddSingleton<IHandlerService, HandlerService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddTransient<ICryptoService, CryptoService>();
        builder.Services.AddTransient<IDroneService, DroneService>();
        builder.Services.AddTransient<ITaskService, TaskService>();
        builder.Services.AddTransient<IPayloadService, PayloadService>();

        // automapper
        builder.Services.AddAutoMapper(typeof(Program));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<HubService>("/SharpC2");
        app.Run();
    }

    private static void ConfigureControllers(MvcOptions opts)
    {
        opts.Filters.Add<JumpCommandFilter>();
    }

    private static string ReadPassword()
    {
        var input = string.Empty;
        
        ConsoleKey key;
        
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && input.Length > 0)
            {
                Console.Write("\b \b");
                input = input[..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                Console.Write("*");
                input += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);

        return input;
    }
    
    private static byte[] GenerateJwtKey(string password)
    {
        using var pbkdf = new Rfc2898DeriveBytes(password, 16, 50000, HashAlgorithmName.SHA256);
        return pbkdf.GetBytes(32);
    }
}