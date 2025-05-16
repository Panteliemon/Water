using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WaterServer.DataAccess;
using WaterServer.Services;
using WaterServer.Utils;

namespace WaterServer;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        string secretsFolderPath = builder.Configuration[
            builder.Environment.IsDevelopment() ? "App:SecretsPath:Home" : "App:SecretsPath:EC2"
        ];

        if (!builder.Environment.IsDevelopment())
        {
            // Dotnet doesn't load CRT/KEY PEM certificates if they were created
            // through certificate signing request. It only parses self-signed certificates
            // created in one step. So it obviously doesn't work with CA-signed certs bought for money.
            // Thankfully, works with PFX format. Here in code both approaches are left.
            X509Certificate2 certificate = null;
            try
            {
                // The way I prefer
                string certPath = Path.Combine(secretsFolderPath, builder.Configuration["App:CertBaseName"] + ".crt");
                string keyPath = Path.Combine(secretsFolderPath, builder.Configuration["App:CertBaseName"] + ".key");
                certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            }
            catch (Exception ex)
            {
                // PFX way
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[x] ");
                Console.ForegroundColor = prevColor;
                Console.WriteLine("crt/key certificate pair - .net REFUSED to load (of course). Trying pfx.");

                string pfxPath = Path.Combine(secretsFolderPath, builder.Configuration["App:CertBaseName"] + ".pfx");
                certificate = new X509Certificate2(pfxPath, "");
            }

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 1 << 20;
                options.Listen(IPAddress.Any, 443, listenOptions =>
                {
                    listenOptions.UseHttps(certificate);
                });
                options.Listen(IPAddress.Any, 80);
            });
        }

        AddServices(builder.Services);
        DataAccessServices.AddServices(builder.Services);

        WebApplication app = builder.Build();
        // To load parameters and display possible warnings immediately
        app.Services.GetRequiredService<IWaterConfig>();

        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.AddCustomAuthenticator();
        app.MapControllers();
        app.AddSimpleAuthorizator();
        app.IntegrateMegaDechunker();

        app.Run();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddSingleton<IWaterConfig, WaterConfig>();
        services.AddSingleton<ICriticalSection, CriticalSection>();
    }
}
