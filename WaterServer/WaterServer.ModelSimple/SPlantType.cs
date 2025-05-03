using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public enum SPlantType
{
    Unused = 0,
    Tomato = 1,
    CayennePepper = 2,
    /// <summary>
    /// Pours to drain (not a plant)
    /// </summary>
    Drain = 255
}
