using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Xml;

namespace WaterServer.DataAccess.Repositories;

internal class SimpleFileBasedRepository : IRepository
{
    private static readonly TimeSpan cacheLifeTime = TimeSpan.FromMinutes(2);
    private object lockObj = new();

    private string storageFilePath;
    private Task criticalSectionTask;

    private SModel cachedModel;
    private DateTime? cacheValidUntil;

    public SimpleFileBasedRepository(IWaterConfig waterConfig)
    {
        storageFilePath = Path.Combine(waterConfig.StorageRoot, "wsdata.xml");
    }

    public async Task<SModel> ReadAll()
    {
        SModel result = null;
        await ExecuteUnderCriticalSection(() => Task.Run(() =>
        {
            if (cacheValidUntil.HasValue)
            {
                DateTime now = DateTime.Now;
                if (now < cacheValidUntil.Value)
                {
                    result = cachedModel?.Clone();
                    return;
                }
            }

            // Fallback: honest read
            if (!File.Exists(storageFilePath))
                return;

            using (StreamReader sr = new StreamReader(
                new FileStream(storageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 262144)))
            {
                result = ModelXml.ReadRoot(sr);
            }

            cacheValidUntil = DateTime.Now.Add(cacheLifeTime);
            cachedModel = result?.Clone();
        }));

        return result;
    }

    public async Task WriteAll(SModel model)
    {
        await ExecuteUnderCriticalSection(() => Task.Run(() =>
        {
            model ??= SModel.Empty();

            cacheValidUntil = DateTime.Now.Add(cacheLifeTime);
            cachedModel = model.Clone();

            using (StreamWriter sw = new StreamWriter(
                new FileStream(storageFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 262144)))
            {
                ModelXml.WriteRoot(model, sw);
            }
        }));
    }

    private async Task ExecuteUnderCriticalSection(Func<Task> funcToExecute)
    {
        TaskCompletionSource tcs = new TaskCompletionSource();
        Task taskToWait = null;

        lock (lockObj)
        {
            if (criticalSectionTask != null)
            {
                taskToWait = criticalSectionTask;
            }

            // We wait for existing task, while the next one will wait for us.
            criticalSectionTask = tcs.Task;
        }

        try
        {
            if (taskToWait != null)
                await taskToWait;

            await funcToExecute();
        }
        finally
        {
            tcs.SetResult();
        }
    }
}
