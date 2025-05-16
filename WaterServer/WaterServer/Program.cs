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
            string certPath = Path.Combine(secretsFolderPath, builder.Configuration["App:CertBaseName"] + ".crt");
            string keyPath = Path.Combine(secretsFolderPath, builder.Configuration["App:CertBaseName"] + ".key");
            string pfxPath = Path.Combine(secretsFolderPath, builder.Configuration["App:CertBaseName"] + ".pfx");
            string passwordPath = Path.Combine(secretsFolderPath, "pass.txt");
            string password = File.ReadAllText(passwordPath).Trim();
            X509Certificate2 certificate = //X509Certificate2.CreateFromPemFile(certPath, keyPath);
                new X509Certificate2(pfxPath, "");

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 1 << 20;
                options.Listen(IPAddress.Any, 443, listenOptions =>
                {
                    listenOptions.UseHttps(certificate);
                });
            });
        }

        AddServices(builder.Services);
        DataAccessServices.AddServices(builder.Services);

        WebApplication app = builder.Build();
        // To load parameters and display possible warnings immediately
        app.Services.GetRequiredService<IWaterConfig>();
        
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
