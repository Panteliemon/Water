using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model;

/// <summary>
/// Instruction for the hardware
/// </summary>
public class WCommand
{
    /// <summary>
    /// Valve index
    /// </summary>
    public byte Valve { get; set; }
    /// <summary>
    /// Amount of water in ml
    /// </summary>
    public short AmountMl { get; set; }
    /// <summary>
    /// Don't pour if overflow / break in the middle of pouring if overflow detected
    /// </summary>
    public bool StopOnOverflow { get; set; }
}
