using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

[XmlRoot(ElementName = "STask")]
public class TaskDto
{
    [XmlAttribute]
    public int Id { get; set; }
    [XmlAttribute]
    public DateTime UtcValidFrom { get; set; }
    [XmlAttribute]
    public DateTime UtcValidTo { get; set; }
    //[XmlArrayItem(ElementName = "Plant")]
    [XmlElement(ElementName = "Plant")]
    public List<TaskItemDto> Items { get; set; }
}
