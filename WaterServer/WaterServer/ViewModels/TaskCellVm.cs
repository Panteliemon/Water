using WaterServer.ModelSimple;

namespace WaterServer.ViewModels;

public class TaskCellVm
{
    public bool ContainsData { get; set; }
    public int VolumeMl { get; set; }
    public STaskStatus Status { get; set; }
}
