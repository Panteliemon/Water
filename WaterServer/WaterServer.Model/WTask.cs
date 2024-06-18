using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model;

public class WTask
{
    public int Id { get; set; }
    /// <summary>
    /// Device which is to perform this task
    /// </summary>
    public WDevice TargetDevice { get; set; }
    /// <summary>
    /// Should be not null
    /// </summary>
    public WCommand Command { get; set; }
    /// <summary>
    /// Beginning of validity interval for the task
    /// </summary>
    public DateTime? UtcFrom { get; set; }
    /// <summary>
    /// Ending of validity interval for the task
    /// </summary>
    public DateTime? UtcTo { get; set; }
    /// <summary>
    /// Null until completion of the task
    /// </summary>
    public WResult Result { get; set; }
}
