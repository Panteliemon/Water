using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

public class PlantDto
{
    [XmlAttribute]
    public int Index { get; set; }
    [XmlAttribute]
    public int PlantType { get; set; }
}
