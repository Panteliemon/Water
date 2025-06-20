﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    private HttpClientHandler httpClientsHandler;
    private HttpClient httpClientPublic;
    private HttpClient httpClientPrivate;
    private string apiKey;

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

        httpClientsHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
        };

        httpClientPublic = new HttpClient(httpClientsHandler)
        {
            BaseAddress = new Uri($"https://{serverDomain}"),
        };

        apiKey = File.ReadAllText(Path.Combine(configuration["SecretsPath"], "apikey-console.txt"));
        httpClientPrivate = new HttpClient(httpClientsHandler)
        {
            BaseAddress = new Uri($"https://{serverDomain}"),
        };
        httpClientPrivate.DefaultRequestHeaders.Add("Water2-ApiKey", apiKey);
    }

    public SModel Pull()
    {
        Task<string> task = httpClientPublic.GetStringAsync("/xml");
        string str = task.Result;
        return ModelXml.ParseRoot(str);
    }

    public void AddPlant(SPlant plant)
    {
        string xml = ModelXml.PlantToStr(plant);
        Task<HttpResponseMessage> responseTask = httpClientPrivate.PostAsync("/setup/plants", new StringContent(xml));
        ServerException.ThrowIfError(responseTask.Result);
    }

    public void UpdatePlant(SPlant plant)
    {
        string xml = ModelXml.PlantToStr(plant);
        Task<HttpResponseMessage> responseTask = httpClientPrivate.PutAsync("/setup/plants", new StringContent(xml));
        ServerException.ThrowIfError(responseTask.Result);
    }

    public void DeletePlant(int plantIndex)
    {
        Task<HttpResponseMessage> responseTask = httpClientPrivate.DeleteAsync($"/setup/plants/{plantIndex}");
        ServerException.ThrowIfError(responseTask.Result);
    }

    public int AddTask(STask task)
    {
        string xml = ModelXml.TaskToStr(task);
        Task<HttpResponseMessage> responseTask = httpClientPrivate.PostAsync("/setup/tasks", new StringContent(xml));
        ServerException.ThrowIfError(responseTask.Result);
        string strId = responseTask.Result.Content.ReadAsStringAsync().Result;
        return int.Parse(strId);
    }

    public void UpdateTask(STask task)
    {
        string xml = ModelXml.TaskToStr(task);
        Task<HttpResponseMessage> responseTask = httpClientPrivate.PutAsync("/setup/tasks", new StringContent(xml));
        ServerException.ThrowIfError(responseTask.Result);
    }

    public void DeleteTask(int id)
    {
        Task<HttpResponseMessage> responseTask = httpClientPrivate.DeleteAsync($"/setup/tasks/{id}");
        ServerException.ThrowIfError(responseTask.Result);
    }

    public void SetWaterConsumptionStart(DateTime? utcValue)
    {
        string strValue = utcValue?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        Task<HttpResponseMessage> responseTask = httpClientPrivate.PostAsync("/setup/wcs", new StringContent(strValue));
        ServerException.ThrowIfError(responseTask.Result);
    }
}
