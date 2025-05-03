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
            X509Certificate2 certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);

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

        app.MapControllers();

        app.Run();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<IWaterConfig, WaterConfig>();
    }
}
