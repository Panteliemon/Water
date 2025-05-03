using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public class SClientActivityRec
{
    public DateTime UtcTimeStamp { get; set; }
    public SClientActivityType ActivityType { get; set; }

    public SClientActivityRec Clone()
    {
        return (SClientActivityRec)MemberwiseClone();
    }
}
