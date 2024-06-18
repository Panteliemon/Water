using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model.Repositories;

public interface ITaskRepository
{
    Task<WTask> GetById(int id);

    Task<List<WTask>> GetAllByDevice(int deviceId);
    Task<List<WTask>> GetActiveByDevice(int deviceId, DateTime? dateToCover);
    Task<List<WTask>> GetFinishedByDevice(int deviceId);

    Task<List<WTask>> GetAllByUser(int userId);
    Task<List<WTask>> GetActiveByUser(int userId);
    Task<List<WTask>> GetFinishedByUser(int userId);

    Task Add(WTask task);
    Task Update(WTask task);
    Task Delete(int id);
}
