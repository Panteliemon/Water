using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WaterServer.Xml.Dto;

[XmlRoot(ElementName = "WaterServerData")]
public class RootDto
{
    [XmlArrayItem(ElementName = "Plant")]
    public List<PlantDto> Plants { get; set; }
    [XmlArrayItem(ElementName = "Task")]
    public List<TaskDto> Tasks { get; set; }
    [XmlArrayItem(ElementName = "ClientActivity")]
    public List<ClientActivityDto> ClientActivities { get; set; }
    public ClientActivityDto LastClientActivity { get; set; }
    public string LastCountsPerLiter { get; set; } // Nullable<int> ugly when serialized as null
}
