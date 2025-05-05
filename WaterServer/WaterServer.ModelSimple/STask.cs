using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.ModelSimple;

public class STask
{
    public int Id { get; set; }
    public DateTime UtcValidFrom { get; set; }
    public DateTime UtcValidTo { get; set; }
    public List<STaskItem> Items { get; set; }

    /// <summary>
    /// Doesn't change links to plants!
    /// </summary>
    public STask Clone()
    {
        STask result = (STask)MemberwiseClone();
        result.Items = Items.Select(x => x.Clone()).ToList();
        return result;
    }

    public static STask Empty()
    {
        return new STask()
        {
            Items = new List<STaskItem>()
        };
    }
}
