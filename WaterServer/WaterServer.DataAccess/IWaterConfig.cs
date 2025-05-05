using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.DataAccess;

public interface IWaterConfig
{
    string StorageRoot { get; }
    string ApiKeyConsole { get; }
    string ApiKeyArduino { get; }
}
