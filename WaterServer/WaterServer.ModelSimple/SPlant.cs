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
    public const int MAX_STD_VOLUMEML = STaskItem.MAX_VOLUMEML;
    public const int MAX_OFFSETML = 500;

    /// <summary>
    /// Zero-based
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// 1-based
    /// </summary>
    public int ValveNo => Index + 1;
    public SPlantType PlantType { get; set; }

    public int? StandardVolumeMl { get; set; }
    public int? OffsetMl { get; set; }

    public bool HasStandardVolume => StandardVolumeMl.HasValue || OffsetMl.HasValue;

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

    public int? GetStandardVolume(double multiplier)
    {
        if (HasStandardVolume)
        {
            int result = (int)Math.Round(
                ((double)(StandardVolumeMl ?? 0)) * multiplier + ((double)(OffsetMl ?? 0))
            );

            if (result < 0)
                result = 0;
            else if (result > STaskItem.MAX_VOLUMEML)
                result = STaskItem.MAX_VOLUMEML;

            return result;
        }
        else
        {
            return null;
        }
    }
}
