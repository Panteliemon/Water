using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model;

public class WResult
{
    public WResultCode Code { get; set; }
    public short PouredMl { get; set; }
    /// <summary>
    /// Datetime when this result was obtained
    /// </summary>
    public DateTime UtcTimestamp { get; set; }
}
