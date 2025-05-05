using System;
using System.Threading.Tasks;

namespace WaterServer;

public interface ICriticalSection
{
    /// <summary>
    /// Execute async action within critical section
    /// </summary>
    Task Execute(Func<Task> action);
    /// <summary>
    /// Execute async action within critical section
    /// </summary>
    Task<T> Execute<T>(Func<Task<T>> action);
}
