using System;
using WaterServer.ModelSimple;

namespace WaterServer.ViewModels;

public class ClientActivityRowVm
{
    public DateTime UtcTimeStamp { get; set; }
    public SClientActivityType ActivityType { get; set; }
}
