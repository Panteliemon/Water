using System;
using System.Threading.Tasks;

namespace WaterServer.Services;

public class CriticalSection : ICriticalSection
{
    private Task previousTask;
    private object lockObj = new();

    public async Task Execute(Func<Task> action)
    {
        Task taskToWait = null;
        TaskCompletionSource tcs = new();

        lock (lockObj)
        {
            taskToWait = previousTask;
            previousTask = tcs.Task;
        }

        try
        {
            // Wait for everyone who arrived before us
            if (taskToWait != null)
                await taskToWait;

            await action();
        }
        finally
        {
            tcs.SetResult();
        }
    }

    public async Task<T> Execute<T>(Func<Task<T>> action)
    {
        Task taskToWait = null;
        TaskCompletionSource tcs = new();

        lock (lockObj)
        {
            taskToWait = previousTask;
            previousTask = tcs.Task;
        }

        try
        {
            // Wait for everyone who arrived before us
            if (taskToWait != null)
                await taskToWait;

            return await action();
        }
        finally
        {
            tcs.SetResult();
        }
    }
}
