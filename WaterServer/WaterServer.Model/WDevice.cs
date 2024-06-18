using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model;

public class WDevice
{
    public int Id { get; set; }
    public string Name { get; set; }
    public byte[] KeyHash { get; set; }
    public string Description { get; set; }
    public User Owner { get; set; }
}
