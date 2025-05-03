using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public enum STaskStatus
{
    Unknown = -1,
    NotStarted = 0,
    InProgress = 1,
    Success = 2,
    /// <summary>
    /// Disabled due to the registered flow rate being lower than expected
    /// </summary>
    LowRate = 10,
    /// <summary>
    /// Special case of LowRate for starting up
    /// </summary>
    NoCounter = 11,
    /// <summary>
    /// Unknown error
    /// </summary>
    Error = 99
}
