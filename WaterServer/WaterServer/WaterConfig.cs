using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using WaterServer.DataAccess;

namespace WaterServer;

internal class WaterConfig : IWaterConfig
{
    public WaterConfig(IWebHostEnvironment environment, IConfiguration configuration,
        ILogger<WaterConfig> logger)
    {
        string secretsFolder;
        if (environment.IsDevelopment())
        {
            StorageRoot = configuration["App:Storage:Home"];
            secretsFolder = configuration["App:SecretsPath:Home"];
        }
        else
        {
            StorageRoot = configuration["App:Storage:EC2"];
            secretsFolder = configuration["App:SecretsPath:EC2"];
        }

        try
        {
            string consoleApiKeyFile = Path.Combine(secretsFolder, "apikey-console.txt");
            ApiKeyConsole = File.ReadAllText(consoleApiKeyFile).Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning("Api key for console not found.");
        }

        try
        {
            // Content of file:
            // #define API_KEY "{value}"
            string arduinoApiKeyFile = Path.Combine(secretsFolder, "apikey-orduino.h");
            string allText2 = File.ReadAllText(arduinoApiKeyFile);
            int firstQuotationMarkIndex = allText2.IndexOf('\"');
            int lastQuotationMarkIndex = allText2.LastIndexOf('\"');
            if ((firstQuotationMarkIndex >= 0) && (lastQuotationMarkIndex > firstQuotationMarkIndex))
            {
                ApiKeyArduino = allText2.Substring(firstQuotationMarkIndex + 1, lastQuotationMarkIndex - firstQuotationMarkIndex - 1);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Api key for Arduino not found.");
        }

        try
        {
            string saltFile = Path.Combine(secretsFolder, "salt.txt");
            PasswordSalt = File.ReadAllText(saltFile).Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning("Password salt not found.");
        }
    }

    public string StorageRoot { get; private set; }
    public string ApiKeyConsole { get; private set; }
    public string ApiKeyArduino { get; private set; }
    public string PasswordSalt { get; private set; }
}
