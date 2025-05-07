using WaterServer.ModelSimple;

namespace WaterServer.Models;

public struct ClientTaskResultSegment
{
    public int PlantIndex { get; }
    public STaskStatus Status { get; }

    public ClientTaskResultSegment(int plantIndex, STaskStatus status)
    {
        PlantIndex = plantIndex;
        Status = status;
    }
}
