using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model;

public enum WResultCode : byte
{
    Ok = 0,
    /// <summary>
    /// Stopped on overflow
    /// </summary>
    Overflow = 1,
    /// <summary>
    /// No signal from flow meter
    /// </summary>
    NoCounter = 2,
    /// <summary>
    /// Flow rate was significantly less than normal
    /// </summary>
    LowRate = 3,
    /// <summary>
    /// Stopped on EStop
    /// </summary>
    EStop = 4
}
