using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Xml;

namespace WaterConsole;

internal class Connector
{
    private string serverDomain;
    private HttpClient httpClient;

    public Connector(IConfiguration configuration)
    {
        if (configuration["Debug"] == "1")
        {
            serverDomain = configuration["WaterServerDomain:Debug"];
        }
        else
        {
            serverDomain = configuration["WaterServerDomain:Release"];
        }

        httpClient = new HttpClient()
        {
            BaseAddress = new Uri($"https://{serverDomain}"),
        };
    }

    public SModel Pull()
    {
        Task<string> task = httpClient.GetStringAsync("/xml");
        string str = task.Result;
        return ModelXml.ParseRoot(str);
    }

    public void AddPlant(SPlant plant)
    {
        string xml = ModelXml.PlantToStr(plant);
        Task<HttpResponseMessage> responseTask = httpClient.PostAsync("/setup/plants", new StringContent(xml));
        ServerException.ThrowIfError(responseTask.Result);
    }
}
