using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

public class TaskItemDto
{
    [XmlAttribute(AttributeName = "Index")]
    public int PlantIndex { get; set; }
    [XmlAttribute]
    public int VolumeMl { get; set; }
    [XmlAttribute]
    public int Status { get; set; }
}
