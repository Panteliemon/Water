using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WaterServer.DataAccess;

namespace WaterServer;

internal class WaterConfig : IWaterConfig
{
    public WaterConfig(IWebHostEnvironment environment, IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            StorageRoot = configuration["App:Storage:Home"];
        }
        else
        {
            StorageRoot = configuration["App:Storage:EC2"];
        }
    }

    public string StorageRoot { get; private set; }
}
