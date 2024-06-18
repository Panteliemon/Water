using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model.Repositories;

public interface IDeviceRepository
{
    Task<WDevice> GetById(int id);
    Task<WDevice> GetByName(string name);

    Task Add(WDevice device);
    Task Update(WDevice device);
    Task Delete(int id);
}
