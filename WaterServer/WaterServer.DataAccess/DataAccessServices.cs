using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.DataAccess.Repositories;
using WaterServer.ModelSimple;

namespace WaterServer.DataAccess;
public static class DataAccessServices
{
    public static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<IRepository, SimpleFileBasedRepository>();
    }
}
