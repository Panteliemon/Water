using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public interface IRepository
{
    Task<SModel> ReadAll();
    Task WriteAll(SModel model);

    Task CreateUser(string name, string password);
    Task<bool> VerifyUser(string name, string password);
}
