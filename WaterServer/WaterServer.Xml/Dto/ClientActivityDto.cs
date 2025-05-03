using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

public class ClientActivityDto
{
    [XmlAttribute]
    public DateTime UtcTimeStamp { get; set; }
    [XmlAttribute]
    public int ActivityType { get; set; }
}
