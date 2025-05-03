using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public class SPlant
{
    public const int MAX_COUNT = 8;
    public const int MAX_INDEX = MAX_COUNT - 1;

    /// <summary>
    /// Zero-based
    /// </summary>
    public int Index { get; set; }
    public SPlantType PlantType { get; set; }

    public static SPlant Empty(int index)
    {
        return new SPlant()
        {
            Index = index,
            PlantType = SPlantType.Unused
        };
    }

    public SPlant Clone()
    {
        return (SPlant)MemberwiseClone();
    }
}
