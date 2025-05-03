using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public class STaskItem
{
    public const int MAX_VOLUMEML = 3000;

    public SPlant Plant { get; set; }
    public int VolumeMl { get; set; }
    public STaskStatus Status { get; set; }

    /// <summary>
    /// Doesn't change link to plant!
    /// </summary>
    public STaskItem Clone()
    {
        STaskItem result = (STaskItem)MemberwiseClone();
        return result;
    }
}
